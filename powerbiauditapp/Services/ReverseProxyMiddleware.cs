// ReverseProxyApplication/ReverseProxyMiddleware.cs
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AppOwnsData
{
    public class ReverseProxyMiddleware
    {
        private static HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly RequestDelegate _nextMiddleware;

        public ReverseProxyMiddleware(RequestDelegate nextMiddleware, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ReverseProxyMiddleware>();
            _logger.LogInformation("Hello");
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            };
            _httpClient = new HttpClient(handler);

            _nextMiddleware = nextMiddleware;
        }

        public async Task Invoke(HttpContext context)
        {
            var targetUri = BuildTargetUri(context.Request);

            if (targetUri != null)
            {
                var targetRequestMessage = CreateTargetMessage(context, targetUri);

                if (context.Request.Path.ToString().Contains("querydata"))
                {
                    _logger.LogWarning("Received QueryData Request");
                }

                _logger.LogInformation("MiddleWare processing: " + context.Request.Path.Value.ToString());
                using var responseMessage = _httpClient.SendAsync(targetRequestMessage).Result;
                _logger.LogInformation("Begin Copy From Target");
                context.Response.StatusCode = (int)responseMessage.StatusCode;
                CopyFromTargetResponseHeaders(context, responseMessage);
                _logger.LogInformation("Begin Process Response");
                await ProcessResponseContent(context, responseMessage);
                return;
            }

            await _nextMiddleware(context);
        }

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);


            return requestMessage;
        }

        private void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
              !HttpMethods.IsHead(requestMethod) &&
              !HttpMethods.IsDelete(requestMethod) &&
              !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                var streamString = streamContent.ReadAsStringAsync().Result;
                requestMessage.Content = new StringContent(streamString);
                requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
            }

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());

            }
            if (context.Request.Path.ToString().Contains("querydata"))
            {
                _logger.LogWarning("Received QueryData Request");
            }

        }

        private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            context.Response.Headers.Remove("transfer-encoding");
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

        private Uri BuildTargetUri(HttpRequest request)
        {
            Uri targetUri = null;

            string Query = request.QueryString.ToString();
            PathString remaining = new PathString();
            bool handled = false;

            if (request.Path.StartsWithSegments("/view") && !handled)
            {
                targetUri = new Uri("https://app.powerbi.com/" + request.Path);
                handled = true;
            }

            if (request.Path.StartsWithSegments("/ssaspbi") && !handled)
            {
                request.Path.StartsWithSegments("/ssaspbi", out remaining);
                targetUri = new Uri("https://wabi-australia-east-a-primary-redirect.analysis.windows.net/" + remaining);
                handled = true;
            }

            if (request.Path.StartsWithSegments("/papps") && !handled)
            {
                request.Path.StartsWithSegments("/papps", out remaining);
                targetUri = new Uri("https://content.powerapps.com/" + remaining);
                handled = true;
            }

        
            if (!handled)
            {
                targetUri = new Uri("https://app.powerbi.com/" + request.Path);
            }

            return new Uri(targetUri.ToString() + Query);
        }


        /// <summary>
        /// Primary logic for intercepting and modifying responses. Can be cleaned up a lot.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        private async Task ProcessResponseContent(HttpContext context, HttpResponseMessage responseMessage)
        {
            byte[] contentbytes;
            string stringContent;

            //ContentType HTML

            bool processed = false; 

            //HTML
            if (IsContentOfType(responseMessage, "text/html"))
            {
                contentbytes = responseMessage.Content.ReadAsByteArrayAsync().Result;
                stringContent = Encoding.UTF8.GetString(contentbytes);

                var newContent = stringContent.Replace("https://app.powerbi.com", "/apppowerbicom")
                    .Replace("https://wabi-australia-east-a-primary-redirect.analysis.windows.net", "/ssaspbi")
                    .Replace("https://content.powerapps.com", "/papps")
                    .Replace("https://api.powerbi.com", "/ssaspbi"); ;

                
                //Use the default configuration for AngleSharp
                var config = Configuration.Default;
                //Create a new context for evaluating webpages with the given config
                var ascontext = BrowsingContext.New(config);
                //Create a virtual request to specify the document to load (here from our fixed string)
                var document = ascontext.OpenAsync(req => req.Content(newContent)).Result;

                //Inject xhook at the top of the page
                var script = document.CreateElement("script");
                script.SetAttribute("type", "text/javascript");
                script.SetAttribute("src", "/lib/xhook/xhook.js");
                //document.Head.Prepend(script);


                //Inject a custom script... you can make all your DOM changes here
                var xhrhook = @"
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
                script2.TextContent = xhrhook;
                script2.SetAttribute("type", "text/javascript");
                document.Body.AppendChild(script2);

                newContent = document.DocumentElement.OuterHtml;
                  
                SetContentLength(context, newContent);

                await context.Response.WriteAsync(newContent, Encoding.UTF8);
                processed = true;
            }

            //JAVASCRIPT
            if(IsContentOfType(responseMessage, "text/javascript") && !processed)
            {
                contentbytes = responseMessage.Content.ReadAsByteArrayAsync().Result;
                stringContent = Encoding.UTF8.GetString(contentbytes);

                var newContent = stringContent.Replace("https://app.powerbi.com", "/apppowerbicom")
                    .Replace("https://wabi-australia-east-a-primary-redirect.analysis.windows.net", "/ssaspbi")
                    .Replace("https://content.powerapps.com", "/papps")
                    .Replace("https://api.powerbi.com", "/ssaspbi"); ;

             
                //Re-Write XHR requests
                if(context.Request.Path.StartsWithSegments("/reportEmbed"))
                {
                    newContent = newContent.Replace("xhr.open(\"GET\", url);", "console.log(url);url=url.replace(\"https://WABI-AUSTRALIA-EAST-A-PRIMARY-redirect.analysis.windows.net\", \"/ssaspbi\");xhr.open(\"GET\", url);");
                    newContent = newContent.Replace("xhr.open(\"POST\", url);", "console.log(url);url=url.replace(\"https://WABI-AUSTRALIA-EAST-A-PRIMARY-redirect.analysis.windows.net\", \"/ssaspbi\");xhr.open(\"POST\", url);");
                }

                SetContentLength(context, newContent);

                await context.Response.WriteAsync(newContent,Encoding.UTF8);
                processed = true;
            }

            if (IsContentOfType(responseMessage, "application/json") && !processed)
            {
                stringContent = responseMessage.Content.ReadAsStringAsync().Result;
                if (context.Request.Path.ToString().Contains("querydata"))
                {
                    //Audit both user and query data returned.
                    var usr = context.User.Identity.Name;
                    stringContent = System.Web.HttpUtility.UrlDecode(stringContent);
                    var j = JObject.Parse(stringContent);
                    JObject newj = new JObject();
                    newj["User"] = usr;
                    newj["Response"] = j;

                    var dt = "audit/" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + Guid.NewGuid().ToString() + ".json";
                    using (System.IO.StreamWriter writer = System.IO.File.CreateText(dt))
                    {
                        writer.Write(JsonConvert.SerializeObject(newj));
                    }
                }
                SetContentLength(context, stringContent);

                await context.Response.WriteAsync(stringContent, Encoding.UTF8);
                processed = true;
            }


            //ALL ELSE    
            if (!processed)
            {
                contentbytes = responseMessage.Content.ReadAsByteArrayAsync().Result;
                await context.Response.Body.WriteAsync(contentbytes);
                processed = true;
            }


           

           
           
        }

        private void SetContentLength(HttpContext context, string Content)
        {
            var contentbyte = Encoding.UTF8.GetBytes(Content);
            var contentLength = Buffer.ByteLength(contentbyte);

            if (context.Response.Headers.ContainsKey("Content-Length"))
            {
                context.Response.Headers["Content-Length"] = contentLength.ToString();
            }
            else
            {
                context.Response.Headers.Add("Content-Length", contentLength.ToString());
            }
        }
        private bool IsContentOfType(HttpResponseMessage responseMessage, string type)
        {
            var result = false;

            if (responseMessage.Content?.Headers?.ContentType != null)
            {
                result = responseMessage.Content.Headers.ContentType.MediaType == type;
            }

            return result;
        }
    }
}