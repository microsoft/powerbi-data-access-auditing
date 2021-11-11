using System.Runtime.InteropServices;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services;

public interface IPowerBiEmbeddedReportService
{
    /// <summary>
    /// Get embed params for a report
    /// </summary>
    /// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for single report</returns>
    Task<ReportParameters> GetReportParameters(ReportDetail report, [Optional] Guid additionalDatasetId);
}