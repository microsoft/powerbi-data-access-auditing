// ReverseProxyApplication/ReverseProxyMiddleware.cs

using AngleSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PowerBiAuditApp.Extensions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace PowerBiAuditApp.Middleware;

public class PowerBiReverseProxyMiddleware
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly RequestDelegate _nextMiddleware;

    public PowerBiReverseProxyMiddleware(RequestDelegate nextMiddleware, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PowerBiReverseProxyMiddleware>();
        _logger.LogInformation("Hello");
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };
        _httpClient = new HttpClient(handler);

        _nextMiddleware = nextMiddleware;
    }

    public async Task Invoke(HttpContext httpContext)
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

        _logger.LogInformation("MiddleWare processing: " + httpContext.Request.Path.Value);
        using var responseMessage = _httpClient.SendAsync(targetRequestMessage).Result;
        _logger.LogInformation("Begin Copy From Target");
        httpContext.Response.StatusCode = (int)responseMessage.StatusCode;
        CopyFromTargetResponseHeaders(httpContext, responseMessage);
        _logger.LogInformation("Begin Process Response");
        await ProcessResponseContent(httpContext, responseMessage);
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
            var streamContent = new StreamContent(httpContext.Request.Body);
            var streamString = streamContent.ReadAsStringAsync().Result;
            requestMessage.Content = new StringContent(streamString);
            requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
        }

        foreach (var (key, value) in httpContext.Request.Headers)
        {
            requestMessage.Headers.TryAddWithoutValidation(key, value.ToArray());

        }

    }

    private void CopyFromTargetResponseHeaders(HttpContext httpContext, HttpResponseMessage responseMessage)
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

    private Uri BuildTargetUri(HttpRequest httpRequest)
    {
        var match = Regex.Match(httpRequest.Path, @"^/power-bi/(?<prefix>[^/]*)(?<remaining>.*$)");
        if (match.Success)
            return new Uri($"https://{match.Groups["prefix"]}.powerbi.com{match.Groups["remaining"]}{httpRequest.QueryString}");

        match = Regex.Match(httpRequest.Path, @"^/analysis/(?<prefix>[^/]*)(?<remaining>.*$)");
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
    /// <returns></returns>
    private async Task ProcessResponseContent(HttpContext httpContext, HttpResponseMessage responseMessage)
    {
        byte[] contentBytes;
        string stringContent;


        //HTML
        if (IsContentOfType(responseMessage, "text/html"))
        {
            contentBytes = responseMessage.Content.ReadAsByteArrayAsync().Result;
            stringContent = Encoding.UTF8.GetString(contentBytes);


            //Use the default configuration for AngleSharp
            var config = Configuration.Default;
            //Create a new context for evaluating webpages with the given config
            var angelSharpContext = BrowsingContext.New(config);
            //Create a virtual request to specify the document to load (here from our fixed string)
            var document = await angelSharpContext.OpenAsync(req => req.Content(ReplaceUrls(stringContent)));

            //Inject xhook at the top of the page
            var script = document.CreateElement("script");
            script.SetAttribute("type", "text/javascript");
            script.SetAttribute("src", "/lib/xhook/xhook.js");


            //Inject a custom script... you can make all your DOM changes here
            var xhrHook = @"
                (function(open)
                {
                    XMLHttpRequest.prototype.open = function()
                    {
                        if(new RegExp(/https\:\/\/[^\/]*\.analysis\.windows\.net/).test(arguments[1])) {
                            arguments[1] = arguments[1].replace(/https\:\/\/([^\/]*)\.analysis\.windows\.net/,'/analysis/$1');
                        }
                        else if (arguments[1].startsWith('https://') && arguments[1] !== 'https://dc.services.visualstudio.com/v2/track' && !arguments[1].startsWith('https://localhost')) {
                            console.log('Failure', arguments)
                        }
                        var result = open.apply(this, arguments);
                        return result;
                    };
                })(XMLHttpRequest.prototype.open);";


            var script2 = document.CreateElement("script");
            script2.TextContent = xhrHook;
            script2.SetAttribute("type", "text/javascript");
            document.Body?.AppendChild(script2);

            var newContent = document.DocumentElement.OuterHtml;

            SetContentLength(httpContext, newContent);

            await httpContext.Response.WriteAsync(newContent, Encoding.UTF8);
            return;
        }

        //JAVASCRIPT
        if (IsContentOfType(responseMessage, "text/javascript"))
        {
            contentBytes = responseMessage.Content.ReadAsByteArrayAsync().Result;
            stringContent = Encoding.UTF8.GetString(contentBytes);

            var newContent = ReplaceUrls(stringContent);

            SetContentLength(httpContext, newContent);

            await httpContext.Response.WriteAsync(newContent, Encoding.UTF8);
            return;
        }

        // DATA
        if (IsContentOfType(responseMessage, "application/json"))
        {
            stringContent = responseMessage.Content.ReadAsStringAsync().Result;
            if (httpContext.Request.Path.ToString().Contains("querydata"))
            {
                //Audit both user and query data returned.
                stringContent = System.Web.HttpUtility.UrlDecode(stringContent);
                var response = new
                {
                    User = httpContext.User.Identity?.Name,
                    Response = JObject.Parse(stringContent)
                };

                var dt = "audit/" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + Guid.NewGuid() + ".json";
                Directory.CreateDirectory("audit");
                await using var writer = File.CreateText(dt);
                await writer.WriteAsync(JsonConvert.SerializeObject(response));
            }
            SetContentLength(httpContext, stringContent);

            await httpContext.Response.WriteAsync(stringContent, Encoding.UTF8);
            return;
        }


        //ALL ELSE    
        contentBytes = responseMessage.Content.ReadAsByteArrayAsync().Result;
        await httpContext.Response.Body.WriteAsync(contentBytes);
    }

    private static string ReplaceUrls(string stringContent)
    {
        return stringContent
            // ReSharper disable StringLiteralTypo
            .RegexReplace(@"https\:\/\/([^/]*)\.powerbi\.com", "/power-bi/$1", RegexOptions.IgnoreCase)
            .RegexReplace(@"https\:\/\/([^/]*)\.analysis\.windows\.net", "/analysis/$1", RegexOptions.IgnoreCase)
            .RegexReplace(@"https\:\/\/([^/]*)\.powerapps\.com", "/power-apps/$1", RegexOptions.IgnoreCase);
        // ReSharper restore StringLiteralTypo
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
    private static bool IsContentOfType(HttpResponseMessage responseMessage, string type)
    {
        var result = false;

        if (responseMessage.Content.Headers.ContentType != null)
        {
            result = responseMessage.Content.Headers.ContentType.MediaType == type;
        }

        return result;
    }
}