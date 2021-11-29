using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
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
                Reports = (await _reportDetailsService.GetReportDetails())
                    .OrderBy(x => x.GroupName)
                    .ThenBy(x => x.Name)
                    .GroupBy(x => x.GroupName)
                    .ToDictionary(x => x.Key, x => x.ToArray())
            };

            return View(model);
        }


        public async Task<List<AadGroup>> GetSecurityGroups(string term) => await _graphService.QueryGroups(term);

        public async Task<IActionResult> RefreshReports()
        {
            var filename = $"{Guid.NewGuid()} {DateTime.UtcNow:yyyy-MM-dd hh-mm-ss}.json";

            await _queueTriggerService.SendQueueMessage(filename, HttpContext.RequestAborted);

            TempData["refreshed"] = true;

            return RedirectToAction(nameof(Index));
        }

        public async Task<IList<ReportDetail>> SaveReportDisplayDetails(string query)
        {
            // Sample error
            // throw new NullReferenceException();

            var queryParameters = query is null ? new NameValueCollection() : HttpUtility.ParseQueryString(query);

            var reports = await _reportDetailsService.GetReportDetails();
            foreach (var report in reports)
            {
                report.Enabled = queryParameters.Get(report.ReportId + "Enabled") == "show";
                report.Description = queryParameters.Get(report.ReportId + "Description");
                report.ReportRowLimit = int.TryParse(queryParameters.Get(report.ReportId + "RowLimit"), out var tempVal) ? tempVal : null;
                report.EffectiveIdentityOverRide = queryParameters.Get(report.ReportId + "EffectiveIdOverride");
                report.AadGroups = JsonConvert.DeserializeObject<AadGroup[]>($"[{queryParameters.Get(report.ReportId + "SecurityGroups")}]");
            }

            await _reportDetailsService.SaveReportDisplayDetails(reports, HttpContext.RequestAborted);

            return reports;
        }
    }
}
