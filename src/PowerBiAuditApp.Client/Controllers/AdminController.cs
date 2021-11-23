using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Client.Security;
using PowerBiAuditApp.Client.Services;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Controllers
{

    [AdministratorAuthorize]
    [AuthorizeForScopes(Scopes = new[] { "Group.Read.All" })]
    public class AdminController : Controller
    {
        private readonly IQueueTriggerService _queueTriggerService;
        private readonly IGraphService _graphService;
        private readonly IReportDetailsService _reportDetailsService;

        public AdminController(IQueueTriggerService queueTriggerService, IGraphService graphService, IReportDetailsService reportDetailsService)
        {
            _queueTriggerService = queueTriggerService;
            _graphService = graphService;
            _reportDetailsService = reportDetailsService;
        }

        public async Task<IActionResult> Index()
        {
            //Make sure we aren't going to see any errors on post back (expect exception and call back if token has expired this is caught and renewed by the AuthorizeForScopes attribute).
            await _graphService.EnsureRequiredScopes();

            var model = new AdminViewModel {
                Reports = await _reportDetailsService.GetAllReportDetails(),
            };

            return View(model);
        }

        public async Task<IActionResult> RefreshReports()
        {
            var filename = $"{Guid.NewGuid()} {DateTime.UtcNow:yyyy-MM-dd hh-mm-ss}.json";

            await _queueTriggerService.SendQueueMessage(filename, HttpContext.RequestAborted);

            TempData["refreshed"] = true;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IList<ReportDetail>> SaveReportDisplayDetails(string query)
        {
            // Example graph service call 
            //var bob = await _graphService.GetGroupIds("City of Bunbury - Projects", "Regis Resources - Projects");

            var queryParameters = query is null ? new NameValueCollection() : HttpUtility.ParseQueryString(query);

            var reports = await _reportDetailsService.GetAllReportDetails();
            foreach (var report in reports)
            {
                report.Enabled = queryParameters.Get(report.Name) == "show";
            }

            await _reportDetailsService.SaveReportDisplayDetails(reports, HttpContext.RequestAborted);

            return reports;
        }
    }
}
