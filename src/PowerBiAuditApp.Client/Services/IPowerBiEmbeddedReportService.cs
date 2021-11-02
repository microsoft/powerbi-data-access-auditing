using System.Runtime.InteropServices;
using PowerBiAuditApp.Client.Models;

namespace PowerBiAuditApp.Client.Services;

public interface IPowerBiEmbeddedReportService
{
    /// <summary>
    /// Get embed params for a report
    /// </summary>
    /// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for single report</returns>
    ReportParameters GetReportParameters(ReportDetails report, [Optional] Guid additionalDatasetId, [Optional] string? effectiveUserName);
}