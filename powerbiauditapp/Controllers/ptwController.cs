// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// ----------------------------------------------------------------------------

namespace AppOwnsData.Controllers
{
    using AngleSharp;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Net.Http;

    public class ptwController : Controller
    {
        public IActionResult Index(string r)
        {
            var usr = HttpContext.User.Identity;

            ViewData.Add("User", usr.Name);
            //ViewData.Add("EmbedToken", embedParams.EmbedToken.Token);
            //ViewData.Add("EmbedURL", embedParams.EmbedReport[0].EmbedUrl);

            HttpClient client = new HttpClient();
            var uri = $"https://app.powerbi.com/view?r={r}";
            var result = client.GetAsync(new Uri(uri)).Result;
            var resultstring = result.Content.ReadAsStringAsync().Result;

            //Use the default configuration for AngleSharp
            var config = Configuration.Default;
            //Create a new context for evaluating webpages with the given config
            var context = BrowsingContext.New(config);
            //Create a virtual request to specify the document to load (here from our fixed string)
            var document = context.OpenAsync(req => req.Content(resultstring)).Result;
            foreach (var s in document.QuerySelectorAll("script"))
            {
                if (s.HasAttribute("src"))
                {
                    var src = s.GetAttribute("src");
                    if (!src.StartsWith("https://"))
                    {
                        s.SetAttribute("src", "https://app.powerbi.com/" + src);
                    }
                }
            }

            var script = document.CreateElement("script");
            script.TextContent = " setTimeout(function(){ $(\".socialSharing\").remove(); }, 3000);";
            script.SetAttribute("type", "text/javascript");
            document.Body.AppendChild(script);
            ViewBag.HtmlStr = document.DocumentElement.OuterHtml;

            return View();
        }
    }
}
