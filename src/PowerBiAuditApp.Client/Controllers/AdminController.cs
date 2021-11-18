using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Web;
using System;
using System.Threading.Tasks;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Client.Services;
using PowerBiAuditApp.Models;
using System.Collections.Generic;

namespace PowerBiAuditApp.Client.Controllers
{
    public class AdminController : Controller
    {
        private readonly IQueueTriggerService _queueTriggerService;
        private readonly IReportDetailsService _reportDetailsService;

        public AdminController(IQueueTriggerService queueTriggerService, IReportDetailsService reportDetailsService)
        {
            _queueTriggerService = queueTriggerService;
            _reportDetailsService = reportDetailsService;
        }

        public async Task<IActionResult> Index()
        {
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
        public async Task<IList<ReportDetail>> SaveReportDisplayDetails(string query) {
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
