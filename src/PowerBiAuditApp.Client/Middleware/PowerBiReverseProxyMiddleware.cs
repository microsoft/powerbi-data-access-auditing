using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PowerBiAuditApp.Client.Extensions;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Client.Services;

namespace PowerBiAuditApp.Client.Middleware
{
    public class PowerBiReverseProxyMiddleware
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly RequestDelegate _nextMiddleware;
        private readonly IDataProtector _dataProtector;

        public PowerBiReverseProxyMiddleware(RequestDelegate nextMiddleware, IDataProtectionProvider dataProtectionProvider, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PowerBiReverseProxyMiddleware>();
            _logger.LogInformation("Hello");
            var handler = new HttpClientHandler {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            };
            _httpClient = new HttpClient(handler);
            _dataProtector = dataProtectionProvider.CreateProtector(Constants.PowerBiTokenPurpose);

            _nextMiddleware = nextMiddleware;
        }

        public async Task Invoke(HttpContext httpContext, IAuditLogger auditLogger)
        {
            await _nextMiddleware(httpContext);
            if (httpContext.Response.StatusCode != StatusCodes.Status404NotFound)
                return;

            var targetUri = BuildTargetUri(httpContext.Request);

            var targetRequestMessage = CreateTargetMessage(httpContext, targetUri);

            if (httpContext.Request.Path.ToString().Contains("querydata"))
            {
                _logger.LogWarning("Received QueryData Request");
            }

            _logger.LogInformation("MiddleWare processing: {path}", httpContext.Request.Path.Value);
            using var responseMessage = _httpClient.SendAsync(targetRequestMessage).Result;
            _logger.LogInformation("Begin Copy From Target");
            httpContext.Response.StatusCode = (int)responseMessage.StatusCode;
            CopyFromTargetResponseHeaders(httpContext, responseMessage);
            _logger.LogInformation("Begin Process Response");
            await ProcessResponseContent(httpContext, responseMessage, auditLogger);
        }

        private HttpRequestMessage CreateTargetMessage(HttpContext httpContext, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(httpContext, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(httpContext.Request.Method);


            return requestMessage;
        }

        private void CopyFromOriginalRequestContentAndHeaders(HttpContext httpContext, HttpRequestMessage requestMessage)
        {
            var requestMethod = httpContext.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                httpContext.Request.EnableBuffering();
                var streamContent = new StreamContent(httpContext.Request.Body);
                var streamString = streamContent.ReadAsStringAsync().Result;
                requestMessage.Content = new StringContent(streamString);
                requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
            }

            foreach (var (key, value) in httpContext.Request.Headers)
            {
                IEnumerable<string> values = value.ToArray();
                if (key.Equals("authorization", StringComparison.CurrentCultureIgnoreCase))
                {
                    values = values.Select(DecryptToken);
                }

                requestMessage.Headers.TryAddWithoutValidation(key, values);

            }

        }

        private string DecryptToken(string token)
        {
            if (!token.StartsWith("EmbedToken "))
                return token;

            var sanitisedToken = WebUtility.HtmlDecode(token.Replace("EmbedToken ", "", StringComparison.CurrentCultureIgnoreCase));

            var tokenParts = sanitisedToken.Split(".");
            var protectedBytes = Convert.FromBase64String(tokenParts.First());
            var unprotectedBytes = _dataProtector.Unprotect(protectedBytes);
            var unencryptedToken = Encoding.UTF8.GetString(unprotectedBytes);
            tokenParts[0] = unencryptedToken;

            if (tokenParts.Length > 1)
            {

                var additionalData = Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[1])).ReformUrls();
                var additionalBytes = Encoding.UTF8.GetBytes(additionalData);
                tokenParts[1] = Convert.ToBase64String(additionalBytes);
            }

            return $"EmbedToken {WebUtility.HtmlEncode(string.Join(".", tokenParts))}";
        }

        private static void CopyFromTargetResponseHeaders(HttpContext httpContext, HttpResponseMessage responseMessage)
        {
            foreach (var (key, value) in responseMessage.Headers)
            {
                httpContext.Response.Headers[key] = value.ToArray();
            }

            foreach (var (key, value) in responseMessage.Content.Headers)
            {
                httpContext.Response.Headers[key] = value.ToArray();
            }

            httpContext.Response.Headers.Remove("transfer-encoding");
        }
        private static HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }

        private static Uri BuildTargetUri(HttpRequest httpRequest)
        {
            var match = Regex.Match(httpRequest.Path, @"^/power-bi/(?<prefix>[^/]*)(?<remaining>.*$)");
            if (match.Success)
                return new Uri($"https://{match.Groups["prefix"]}.powerbi.com{match.Groups["remaining"]}{httpRequest.QueryString}");

            match = Regex.Match(httpRequest.Path, @"^/analysis-windows/(?<prefix>[^/]*)(?<remaining>.*$)");
            if (match.Success)
                return new Uri($"https://{match.Groups["prefix"]}.analysis.windows.net{match.Groups["remaining"]}{httpRequest.QueryString}");

            match = Regex.Match(httpRequest.Path, @"^/power-apps/(?<prefix>[^/]*)(?<remaining>.*$)");
            if (match.Success)
                return new Uri($"https://{match.Groups["prefix"]}.powerapps.com{match.Groups["remaining"]}{httpRequest.QueryString}");

            if (httpRequest.Path.StartsWithSegments("/reportEmbed"))
                return new Uri("https://app.powerbi.com/" + httpRequest.Path + httpRequest.QueryString);

            return new Uri("https://app.powerbi.com/" + httpRequest.Path + httpRequest.QueryString);
        }


        /// <summary>
        /// Primary logic for intercepting and modifying responses. Can be cleaned up a lot.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="responseMessage"></param>
        /// <param name="auditLogger"></param>
        /// <returns></returns>
        private static async Task ProcessResponseContent(HttpContext httpContext, HttpResponseMessage responseMessage, IAuditLogger auditLogger)
        {
            // Can't modify body in this case.
            if (responseMessage.StatusCode == HttpStatusCode.NotModified)
                return;

            await auditLogger.CreateAuditLog(httpContext, responseMessage);

            //HTML || JAVASCRIPT
            if (responseMessage.IsContentOfType("text/html") || responseMessage.IsContentOfType("text/javascript"))
            {
                var stringContent = responseMessage.Content.ReadAsStringAsync().Result;
                //stringContent = Encoding.UTF8.GetString(contentBytes);

                var newContent = stringContent.ReplaceUrls();


                SetContentLength(httpContext, newContent);

                await httpContext.Response.WriteAsync(newContent, Encoding.UTF8);
                return;
            }

            //ALL ELSE
            var contentBytes = responseMessage.Content.ReadAsByteArrayAsync().Result;
            await httpContext.Response.Body.WriteAsync(contentBytes);
        }


        private static void SetContentLength(HttpContext httpContext, string content)
        {
            var contentByte = Encoding.UTF8.GetBytes(content);
            var contentLength = Buffer.ByteLength(contentByte);

            if (httpContext.Response.Headers.ContainsKey("Content-Length"))
            {
                httpContext.Response.Headers["Content-Length"] = contentLength.ToString();
            }
            else
            {
                httpContext.Response.Headers.Add("Content-Length", contentLength.ToString());
            }
        }
    }
}