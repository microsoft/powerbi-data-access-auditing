using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services
{
    public interface IReportDetailsService
    {
        Task<IList<ReportDetail>> GetReportDetails();
        Task<IList<ReportDetail>> GetAllReportDetailsForUser();
        Task<IList<ReportDetail>> GetEnabledReportDetailsForUser();
        Task<ReportDetail> GetReportDetail(Guid workspaceId, Guid reportId);
        Task<ReportDetail> GetReportForUser(Guid workspaceId, Guid reportId);
        Task SaveReportDisplayDetails(IList<ReportDetail> reports, CancellationToken cancellationToken = default);
    } 
}
