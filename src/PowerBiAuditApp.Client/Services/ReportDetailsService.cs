using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services
{
    public class ReportDetailsService : IReportDetailsService
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string CacheKey = nameof(ReportDetail);
        private const int CacheTime = 30;

        public ReportDetailsService(TableServiceClient tableServiceClient, IHttpContextAccessor httpContextAccessor)
        {
            _tableServiceClient = tableServiceClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<IList<ReportDetail>> GetAllReportDetails() => RetrieveReportDetails();
        public async Task<IList<ReportDetail>> GetReportDetailsForUser()
        {
            var userGroups = _httpContextAccessor.HttpContext?.User.Claims
                .Where(x => x.Type == "groups")
                .Select(x => new Guid(x.Value))
                .ToArray() ?? Array.Empty<Guid>();

            return (await RetrieveReportDetails()).Where(x => x.AadGroups.Any(a => x.Enabled && userGroups.Contains(a))).ToList();
        }

        public async Task<ReportDetail> GetReportDetail(Guid workspaceId, Guid reportId) => (await GetAllReportDetails()).FirstOrDefault(x => x.GroupId == workspaceId && x.ReportId == reportId);
        public async Task<ReportDetail> GetReportForUser(Guid workspaceId, Guid reportId) => (await GetReportDetailsForUser()).FirstOrDefault(x => x.GroupId == workspaceId && x.ReportId == reportId);

        public Task SaveReportDisplayDetails(IList<ReportDetail> reports, CancellationToken cancellationToken) => ModifyReportDetails(reports, cancellationToken);

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

        private async Task ModifyReportDetails(IList<ReportDetail> reports, CancellationToken cancellationToken)
        {
            var tableClient = _tableServiceClient.GetTableClient(nameof(ReportDetail));

            foreach (var report in reports)
            {
                await tableClient.UpdateEntityAsync(report, Azure.ETag.All, TableUpdateMode.Merge, cancellationToken);
            }
        }
    }
}
