using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Client.Services;

namespace PowerBiAuditApp.Client.Controllers
{
    public class ReportController : Controller
    {
        private readonly IPowerBiEmbeddedReportService _powerBiReportService;
        private readonly IReportDetailsService _reportDetailsService;

        public ReportController(IPowerBiEmbeddedReportService powerBiReportService, IReportDetailsService reportDetailsService)
        {
            _powerBiReportService = powerBiReportService;
            _reportDetailsService = reportDetailsService;
        }

        // GET: ReportController
        public async Task<IActionResult> Index(Guid workspaceId, Guid reportId, int pageNumber)
        {

            var report = await _reportDetailsService.GetReportForUser(workspaceId, reportId);
            if (report is null)
                return RedirectToAction("Index", "Home");

            var reportParameters = await _powerBiReportService.GetReportParameters(report);

            return View(new ReportViewModel {
                User = HttpContext.User.Identity?.Name,
                EmbedToken = reportParameters.EmbedToken,
                EmbedUrl = reportParameters.EmbedUrl,
                ReportId = report.ReportId,
                WorkspaceId = report.GroupId,
                PageNumber = Math.Max(Math.Min(pageNumber, reportParameters.Pages.Length - 1), 0),
                Pages = reportParameters.Pages
            });
        }
    }
}