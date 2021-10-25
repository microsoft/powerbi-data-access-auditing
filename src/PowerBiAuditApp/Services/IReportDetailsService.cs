using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Services;

public interface IReportDetailsService
{
    IList<ReportDetails> GetReportDetails();
    ReportDetails? GetReportDetails(Guid workspaceId, Guid reportId);
}