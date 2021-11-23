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
        Task<ReportDetail> GetReportDetail(Guid reportId);
        Task SaveReportDisplayDetails(IList<ReportDetail> reports, CancellationToken cancellationToken = default);
    }
}