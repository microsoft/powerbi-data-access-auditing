using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services
{
    public class ReportDetailsService : IReportDetailsService
    {
        private readonly TableServiceClient _tableServiceClient;

        public ReportDetailsService(TableServiceClient tableServiceClient)
        {
            _tableServiceClient = tableServiceClient;
        }

        public Task<IList<ReportDetail>> GetReportDetails() => RetrieveReportDetails();

        public async Task<ReportDetail> GetReportDetail(Guid reportId) => (await GetReportDetails()).FirstOrDefault(x => x.ReportId == reportId);


        public async Task SaveReportDisplayDetails(IList<ReportDetail> reports, CancellationToken cancellationToken)
        {
            var tableClient = _tableServiceClient.GetTableClient(nameof(ReportDetail));

            foreach (var report in reports)
            {
                await tableClient.UpdateEntityAsync(report, Azure.ETag.All, TableUpdateMode.Merge, cancellationToken);
            }
        }

        private async Task<IList<ReportDetail>> RetrieveReportDetails()
        {
            var reportDetails = new List<ReportDetail>();
            var tableClient = _tableServiceClient.GetTableClient(nameof(ReportDetail));
            await tableClient.CreateIfNotExistsAsync();
            await foreach (var reportDetail in tableClient.QueryAsync<ReportDetail>())
            {
                reportDetails.Add(reportDetail);
            }
            return reportDetails;
        }
    }
}