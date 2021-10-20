using Microsoft.AspNetCore.Mvc;
using PowerBiAuditApp.Services;

namespace PowerBiAuditApp.Controllers;

public class ReportController : Controller
{
    private readonly IPowerBiReportService _powerBiReportService;
    private readonly IReportDetailsService _reportDetailsService;

    public ReportController(IPowerBiReportService powerBiReportService, IReportDetailsService reportDetailsService)
    {
        _powerBiReportService = powerBiReportService;
        _reportDetailsService = reportDetailsService;
    }

    // GET: ReportController
    public IActionResult Index(Guid workspaceId, Guid reportId, int pageNumber)
    {

        var report = _reportDetailsService.GetReportDetails(workspaceId, reportId);
        if (report is null)
            return RedirectToAction("Index", "Home");

        var reportParameters = string.IsNullOrWhiteSpace(HttpContext.User.Identity?.Name)
            ? _powerBiReportService.GetReportParameters(report)
            : _powerBiReportService.GetReportParameters(report, effectiveUserName: null /*HttpContext.User.Identity?.Name!*/);

        var localisedUri = reportParameters.EmbedUrl.Replace("app.powerbi.com", Request.Host.ToString());
        if (!string.IsNullOrWhiteSpace(report.PaginationTable) && !string.IsNullOrWhiteSpace(report.PaginationColumn))
            localisedUri = new Uri($"{localisedUri}&$filter={report.PaginationTable}/{report.PaginationColumn} eq {pageNumber}", UriKind.Absolute).AbsoluteUri;

        ViewData.Add("User", HttpContext.User.Identity?.Name);
        ViewData.Add("EmbedToken", reportParameters.EmbedToken.Token);
        ViewData.Add("EmbedURL", localisedUri);
        ViewData.Add("ReportId", report.ReportId);
        ViewData.Add("WorkspaceId", report.WorkspaceId);
        ViewData.Add("PageNumber", pageNumber);
        ViewData.Add("PaginationTable", report.PaginationTable);

        return View();
    }
}