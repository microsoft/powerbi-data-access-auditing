using Microsoft.AspNetCore.Mvc;
﻿using System.Collections.Specialized;
using System.Web;
﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Client.Services;
using PowerBiAuditApp.Models;

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
                Reports = await _reportDetailsService.GetReportDetailsForUser(),
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
            var test = query is null ? new NameValueCollection() : HttpUtility.ParseQueryString(query);

            var reports = await _reportDetailsService.GetReportDetailsForUser();
            foreach (var report in reports)
            {
                report.Enabled = test.Get(report.Name) == "show";
            }

            await _reportDetailsService.UpdateReportDetails(reports, HttpContext.RequestAborted);

            return reports;
        }
    }
}
