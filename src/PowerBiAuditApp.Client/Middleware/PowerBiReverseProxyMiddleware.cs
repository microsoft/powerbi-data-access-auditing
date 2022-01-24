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
using Newtonsoft.Json.Linq;
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
        private readonly IReportDetailsService _reportDetailsService;

        public PowerBiReverseProxyMiddleware(RequestDelegate nextMiddleware, IDataProtectionProvider dataProtectionProvider, ILoggerFactory loggerFactory, IReportDetailsService reportDetailsService)
        {
            _logger = loggerFactory.CreateLogger<PowerBiReverseProxyMiddleware>();
            _logger.LogInformation("Hello");
            var handler = new HttpClientHandler {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            };
            _httpClient = new HttpClient(handler);
            _dataProtector = dataProtectionProvider.CreateProtector(Constants.PowerBiTokenPurpose);

            _nextMiddleware = nextMiddleware;
            _reportDetailsService = reportDetailsService;
        }

        public async Task Invoke(HttpContext httpContext, IAuditLogger auditLogger)
        {
            await _nextMiddleware(httpContext);
            if (httpContext.Response.StatusCode != StatusCodes.Status404NotFound
                && !(httpContext.Response.StatusCode == StatusCodes.Status302Found && httpContext.Request.Path.ToString().EndsWith(".js"))
                )
                return;

            var targetUri = BuildTargetUri(httpContext.Request);

            var (targetRequestMessage, reportId, reportName) = await CreateTargetMessage(httpContext, targetUri);

            if (httpContext.Request.Path.ToString().Contains("querydata"))
            {
                _logger.LogWarning("Received QueryData Request");
            }

            _logger.LogInformation("MiddleWare processing: {path}", httpContext.Request.Path.Value);
            using var responseMessage = await _httpClient.SendAsync(targetRequestMessage);
            _logger.LogInformation("Begin Copy From Target");
            httpContext.Response.StatusCode = (int)responseMessage.StatusCode;
            CopyFromTargetResponseHeaders(httpContext, responseMessage);
            _logger.LogInformation("Begin Process Response");
            await ProcessResponseContent(httpContext, responseMessage, auditLogger, reportId, reportName);
        }


        private async Task<(HttpRequestMessage requestMessage, Guid? reportId, string reportName)> CreateTargetMessage(HttpContext httpContext, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            var (reportId, reportName) = await CopyFromOriginalRequestContentAndHeaders(httpContext, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(httpContext.Request.Method);


            return (requestMessage, reportId, reportName);
        }

        private async Task<(Guid? reportId, string reportName)> CopyFromOriginalRequestContentAndHeaders(HttpContext httpContext, HttpRequestMessage requestMessage)
        {
            Guid? reportId = null;
            string reportName = null;
            foreach (var (key, headerValue) in httpContext.Request.Headers)
            {
                IEnumerable<string> values = headerValue.ToArray();
                if (key.Equals("authorization", StringComparison.CurrentCultureIgnoreCase))
                {
                    values = values.Select(x =>
                     {
                         var (value, id, name) = DecryptTokenAndReportId(x);
                         reportId ??= id;
                         reportName ??= name;

                         return value;
                     });
                }

                requestMessage.Headers.TryAddWithoutValidation(key, values);
            }

            var requestMethod = httpContext.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                httpContext.Request.EnableBuffering();
                requestMessage.Content = await GetRequestContent(httpContext, reportId);
                requestMessage.Content!.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
            }

            return (reportId, reportName);
        }

        private async Task<StringContent> GetRequestContent(HttpContext httpContext, Guid? reportId)
        {
            var streamBody = new StreamContent(httpContext.Request.Body);
            var body = await streamBody.ReadAsStringAsync();

            if (reportId is not null && httpContext.Request.ContentType.Contains("application/json"))
            {
                var json = JObject.Parse(body);
                foreach (var query in (JArray)json["queries"] ?? new JArray())
                {
                    var report = await _reportDetailsService.GetReportDetail(reportId.Value);
                    if (report.ReportRowLimit is null)
                        continue;

                    foreach (var command in (JArray)query["Query"]?["Commands"] ?? new JArray())
                    {
                        var binding = command["SemanticQueryDataShapeCommand"]?["Binding"] as JObject ?? throw new NullReferenceException();

                        var dataReduction = binding["DataReduction"] as JObject;
                        if (dataReduction is null)
                        {
                            dataReduction = new JObject();
                            binding["DataReduction"] = dataReduction;
                        }

                        SetRowLimit(dataReduction, report.ReportRowLimit.Value, "Primary");
                        SetRowLimit(dataReduction, report.ReportRowLimit.Value, "Secondary");
                    }
                }

                body = json.ToString();
            }
            return new StringContent(body);
        }

        private static void SetRowLimit(JObject dataReduction, int rowLimit, string queryProperty)
        {
            var query = dataReduction[queryProperty] as JObject;
            if (query is null)
                return;

            var limit = query["Window"] as JObject;

            if (limit is null)
                return;

            limit["Count"] = rowLimit;
        }

        private (string token, Guid? reportId, string reportName) DecryptTokenAndReportId(string token)
        {
            if (!token.StartsWith("EmbedToken "))
                return (token, null, null);

            var sanitisedToken = WebUtility.HtmlDecode(token.Replace("EmbedToken ", "", StringComparison.CurrentCultureIgnoreCase));

            var tokenParts = sanitisedToken.Split(".");
            var protectedBytes = Convert.FromBase64String(tokenParts.First());
            var unprotectedBytes = _dataProtector.Unprotect(protectedBytes);
            var unencryptedToken = Encoding.UTF8.GetString(unprotectedBytes);
            tokenParts[0] = unencryptedToken;

            Guid? reportId = null;
            string reportName = null;
            if (tokenParts.Length > 1)
            {
                var additionalData = Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[1])).ReformUrls();

                var json = JObject.Parse(additionalData);
                if (Guid.TryParse(json["reportId"]?.ToString(), out var reportIdTemp))
                    reportId = reportIdTemp;

                reportName = json["reportName"]?.ToString();


                var additionalBytes = Encoding.UTF8.GetBytes(additionalData);
                tokenParts[1] = Convert.ToBase64String(additionalBytes);
            }

            return ($"EmbedToken {WebUtility.HtmlEncode(string.Join(".", tokenParts))}", reportId, reportName);
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
        /// <returns></returns>
        private static async Task ProcessResponseContent(HttpContext httpContext, HttpResponseMessage responseMessage, IAuditLogger auditLogger, Guid? reportId, string reportName)
        {
            // Can't modify body in this case.
            if (responseMessage.StatusCode == HttpStatusCode.NotModified)
                return;

            await auditLogger.CreateAuditLog(httpContext, responseMessage, reportId, reportName);

            //HTML || JAVASCRIPT
            if (responseMessage.IsContentTypeHtml() || responseMessage.IsContentTypeJavaScript())
            {
                var stringContent = responseMessage.Content.ReadAsStringAsync().Result;

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