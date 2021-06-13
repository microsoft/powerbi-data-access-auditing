// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// ----------------------------------------------------------------------------

namespace AppOwnsData.Controllers
{
    using AppOwnsData.Models;
    using AppOwnsData.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using System;


    public class OffenderController : Controller
    {
        private readonly PbiEmbedService pbiEmbedService;
        private readonly IOptions<AzureAd> azureAd;
        private readonly IOptions<PowerBI> powerBI;

        public OffenderController(PbiEmbedService pbiEmbedService, IOptions<AzureAd> azureAd, IOptions<PowerBI> powerBI)
        {
            this.pbiEmbedService = pbiEmbedService;
            this.azureAd = azureAd;
            this.powerBI = powerBI;
        }


        public IActionResult Index(string WorkspaceId, string ReportId, string id)
        {
            if (id == null)
            {
                return NotFound();
            }


            // Validate whether all the required configurations are provided in appsettings.json
            string configValidationResult = ConfigValidatorService.ValidateConfig(azureAd, powerBI);
            EmbedParams embedParams = pbiEmbedService.GetEmbedParams( workspaceId: new Guid(WorkspaceId), reportId: new Guid(ReportId),effectiveUserName: id);


            var usr = HttpContext.User.Identity;
            ViewData.Add("User", usr.Name);
            ViewData.Add("EmbedToken", embedParams.EmbedToken.Token);
            ViewData.Add("EmbedURL", embedParams.EmbedReport[0].EmbedUrl);
            ViewData.Add("Id", id);
            return View();
        }
    }
}
