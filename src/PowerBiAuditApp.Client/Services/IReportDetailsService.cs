using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services;

public interface IReportDetailsService
{
    Task<IList<ReportDetail>> GetReportDetails();
    Task<ReportDetail?> GetReportDetail(Guid workspaceId, Guid reportId);
}