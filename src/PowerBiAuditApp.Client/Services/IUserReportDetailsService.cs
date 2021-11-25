using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services
{
    public interface IUserReportDetailsService
    {
        Task<IList<ReportDetail>> GetReportDetails();
        Task<ReportDetail> GetReportDetail(Guid workspaceId, Guid reportId);
    }
}
