using PowerBiAuditApp.Client.Models;

namespace PowerBiAuditApp.Client.Services;

public interface IReportDetailsService
{
    IList<ReportDetails> GetReportDetails();
    ReportDetails? GetReportDetails(Guid workspaceId, Guid reportId);
}