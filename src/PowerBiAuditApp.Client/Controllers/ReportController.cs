using Microsoft.AspNetCore.Mvc;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Client.Services;

namespace PowerBiAuditApp.Client.Controllers;

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

        var report = await _reportDetailsService.GetReportDetail(workspaceId, reportId);
        if (report is null)
            return RedirectToAction("Index", "Home");

        var reportParameters = string.IsNullOrWhiteSpace(HttpContext.User.Identity?.Name)
            ? _powerBiReportService.GetReportParameters(report)
            : _powerBiReportService.GetReportParameters(report, effectiveUserName: HttpContext.User.Identity?.Name!);

        var localisedUri = reportParameters.EmbedUrl.Replace("app.powerbi.com", Request.Host.ToString());
        if (!string.IsNullOrWhiteSpace(report.PaginationTable) && !string.IsNullOrWhiteSpace(report.PaginationColumn))
            localisedUri = new Uri($"{localisedUri}&$filter={report.PaginationTable}/{report.PaginationColumn} eq {pageNumber}", UriKind.Absolute).AbsoluteUri;


        return View(new ReportViewModel {
            User = HttpContext.User.Identity?.Name,
            EmbedToken = reportParameters.EmbedToken,
            EmbedUrl = localisedUri,
            ReportId = report.ReportId,
            WorkspaceId = report.GroupId,
            PageNumber = pageNumber,
            PaginationTable = report.PaginationTable,
        });
    }
}