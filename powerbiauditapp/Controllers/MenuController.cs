// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// ----------------------------------------------------------------------------

namespace AppOwnsData.Controllers
{
    using AngleSharp;
    using AppOwnsData.Models;
    using AppOwnsData.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using System;
    using System.Net.Http;

    public class MenuController : Controller
    {
        private readonly PbiEmbedService pbiEmbedService;
        private readonly IOptions<AzureAd> azureAd;
        private readonly IOptions<PowerBI> powerBI;

        public MenuController(PbiEmbedService pbiEmbedService, IOptions<AzureAd> azureAd, IOptions<PowerBI> powerBI)
        {
            this.pbiEmbedService = pbiEmbedService;
            this.azureAd = azureAd;
            this.powerBI = powerBI;
        }


        public IActionResult Index()
        {
            // Validate whether all the required configurations are provided in appsettings.json
            string configValidationResult = ConfigValidatorService.ValidateConfig(azureAd, powerBI);

            var usr = HttpContext.User.Identity;
            ViewData.Add("User", usr.Name);
            ViewData.Add("Reports", powerBI.Value.Reports);

            return View();

        }
    }
}
