using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services;

public interface IReportDetailsService
{
    Task<IList<ReportDetail>> GetReportDetails();
    Task<IList<ReportDetail>> GetReportDetailsForUser();
    Task<ReportDetail?> GetReportDetail(Guid workspaceId, Guid reportId);
    Task<ReportDetail?> GetReportForUser(Guid workspaceId, Guid reportId);
}