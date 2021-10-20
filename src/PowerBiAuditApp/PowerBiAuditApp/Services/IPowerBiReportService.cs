using PowerBiAuditApp.Models;
using System.Runtime.InteropServices;

namespace PowerBiAuditApp.Services;

public interface IPowerBiReportService
{
    /// <summary>
    /// Get embed params for a report
    /// </summary>
    /// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for single report</returns>
    ReportParameters GetReportParameters(ReportDetails report, [Optional] Guid additionalDatasetId, [Optional] string? effectiveUserName, [Optional] string effectiveUserRole);
}