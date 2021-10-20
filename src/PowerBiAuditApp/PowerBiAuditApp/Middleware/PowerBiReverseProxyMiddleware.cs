// ReverseProxyApplication/ReverseProxyMiddleware.cs

using AngleSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

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
        if (httpContext.Request.Path.ToString().Contains("querydata"))
        {
            _logger.LogWarning("Received QueryData Request");
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
        var targetUri = new Uri("https://app.powerbi.com/" + httpRequest.Path);


        var handled = httpRequest.Path.StartsWithSegments("/view");

        if (httpRequest.Path.StartsWithSegments("/ssaspbi") && !handled)
        {
            httpRequest.Path.StartsWithSegments("/ssaspbi", out var remaining);
            targetUri = new Uri("https://wabi-australia-east-a-primary-redirect.analysis.windows.net/" + remaining);
            handled = true;
        }

        if (httpRequest.Path.StartsWithSegments("/papps") && !handled)
        {
            httpRequest.Path.StartsWithSegments("/papps", out var remaining);
            targetUri = new Uri("https://content.powerapps.com/" + remaining);
        }


        return new Uri(targetUri + httpRequest.QueryString.ToString());
    }


    /// <summary>
    /// Primary logic for intercepting and modifying responses. Can be cleaned up a lot.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="responseMessage"></param>
    /// <returns></returns>
    private async Task ProcessResponseContent(HttpContext httpContext, HttpResponseMessage responseMessage)
    {
        byte[] contentBytes;
        string stringContent;

        //ContentType HTML

        var processed = false;

        //HTML
        if (IsContentOfType(responseMessage, "text/html"))
        {
            contentBytes = responseMessage.Content.ReadAsByteArrayAsync().Result;
            stringContent = Encoding.UTF8.GetString(contentBytes);

            var newContent = stringContent.Replace("https://app.powerbi.com", "/apppowerbicom")
                .Replace("https://wabi-australia-east-a-primary-redirect.analysis.windows.net", "/ssaspbi")
                .Replace("https://content.powerapps.com", "/papps")
                .Replace("https://api.powerbi.com", "/ssaspbi");


            //Use the default configuration for AngleSharp
            var config = Configuration.Default;
            //Create a new context for evaluating webpages with the given config
            var angelSharpContext = BrowsingContext.New(config);
            //Create a virtual request to specify the document to load (here from our fixed string)
            var document = angelSharpContext.OpenAsync(req => req.Content(newContent)).Result;

            //Inject xhook at the top of the page
            var script = document.CreateElement("script");
            script.SetAttribute("type", "text/javascript");
            script.SetAttribute("src", "/lib/xhook/xhook.js");
            //document.Head.Prepend(script);


            //Inject a custom script... you can make all your DOM changes here
            var xhrHook = @"
                (function(open)
                {
                    XMLHttpRequest.prototype.open = function()
                    {
                        console.log(arguments);
                        if(arguments[1].includes('https://WABI-AUSTRALIA-EAST-A-PRIMARY-redirect.analysis.windows.net'))
                        {
                            arguments[1] = arguments[1].replace('https://WABI-AUSTRALIA-EAST-A-PRIMARY-redirect.analysis.windows.net','/ssaspbi');
                        }
                        console.log(arguments);
                        var result = open.apply(this, arguments);
                        return result;
                    };
                })(XMLHttpRequest.prototype.open);";


            var script2 = document.CreateElement("script");
            script2.TextContent = xhrHook;
            script2.SetAttribute("type", "text/javascript");
            document.Body.AppendChild(script2);

            newContent = document.DocumentElement.OuterHtml;

            SetContentLength(httpContext, newContent);

            await httpContext.Response.WriteAsync(newContent, Encoding.UTF8);
            return;
        }

        //JAVASCRIPT
        if (IsContentOfType(responseMessage, "text/javascript"))
        {
            contentBytes = responseMessage.Content.ReadAsByteArrayAsync().Result;
            stringContent = Encoding.UTF8.GetString(contentBytes);

            var newContent = stringContent.Replace("https://app.powerbi.com", "/apppowerbicom")
                .Replace("https://wabi-australia-east-a-primary-redirect.analysis.windows.net", "/ssaspbi")
                .Replace("https://content.powerapps.com", "/papps")
                .Replace("https://api.powerbi.com", "/ssaspbi"); ;


            //Re-Write XHR requests
            if (httpContext.Request.Path.StartsWithSegments("/reportEmbed"))
            {
                newContent = newContent.Replace("xhr.open(\"GET\", url);", "console.log(url);url=url.replace(\"https://WABI-AUSTRALIA-EAST-A-PRIMARY-redirect.analysis.windows.net\", \"/ssaspbi\");xhr.open(\"GET\", url);");
                newContent = newContent.Replace("xhr.open(\"POST\", url);", "console.log(url);url=url.replace(\"https://WABI-AUSTRALIA-EAST-A-PRIMARY-redirect.analysis.windows.net\", \"/ssaspbi\");xhr.open(\"POST\", url);");
            }

            SetContentLength(httpContext, newContent);

            await httpContext.Response.WriteAsync(newContent, Encoding.UTF8);
            return;
        }

        if (IsContentOfType(responseMessage, "application/json") && !processed)
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