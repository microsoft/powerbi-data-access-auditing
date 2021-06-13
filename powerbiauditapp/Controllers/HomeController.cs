// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// ----------------------------------------------------------------------------

namespace AppOwnsData.Controllers
{
    using AngleSharp;
    using AppOwnsData.Models;
    using AppOwnsData.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using System;
    using System.Net.Http;

    public class HomeController : Controller
    {
        private readonly PbiEmbedService pbiEmbedService;
        private readonly IOptions<AzureAd> azureAd;
        private readonly IOptions<PowerBI> powerBI;

        public HomeController(PbiEmbedService pbiEmbedService, IOptions<AzureAd> azureAd, IOptions<PowerBI> powerBI)
        {
            this.pbiEmbedService = pbiEmbedService;
            this.azureAd = azureAd;
            this.powerBI = powerBI;
        }


        public IActionResult Index(string r, string? WorkspaceId, string? Id)
        {


            if (r == null)
            {
                return RedirectToAction("index", "menu");
            }

            EmbedParams embedParams;
            // Validate whether all the required configurations are provided in appsettings.json
            string configValidationResult = ConfigValidatorService.ValidateConfig(azureAd, powerBI);
            if (Id == null)
            {
                embedParams = pbiEmbedService.GetEmbedParams(workspaceId: new Guid(WorkspaceId), reportId: new Guid(r));
            }
            else
            {
                embedParams = pbiEmbedService.GetEmbedParams(workspaceId: new Guid(WorkspaceId), reportId: new Guid(r), effectiveUserName: Id);
            }

            var usr = HttpContext.User.Identity;
            ViewData.Add("User", usr.Name);
            ViewData.Add("EmbedToken", embedParams.EmbedToken.Token);
            ViewData.Add("EmbedURL", embedParams.EmbedReport[0].EmbedUrl);

            return View();
        }
    }
}
