using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services
{
    public class ReportDetailsService : IReportDetailsService
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportDetailsService(TableServiceClient tableServiceClient, IHttpContextAccessor httpContextAccessor)
        {
            _tableServiceClient = tableServiceClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<IList<ReportDetail>> GetReportDetails() => RetrieveReportDetails();
        public async Task<IList<ReportDetail>> GetReportDetailsForUser()
        {
            var userGroups = _httpContextAccessor.HttpContext?.User.Claims
                .Where(x => x.Type == "groups")
                .Select(x => new Guid(x.Value))
                .ToArray() ?? Array.Empty<Guid>();

            return (await RetrieveReportDetails()).Where(x => x.Enabled && x.AadGroups.Any(a => userGroups.Contains(a))).ToList();
        }

        public async Task<ReportDetail> GetReportDetail(Guid workspaceId, Guid reportId) => (await GetReportDetails()).FirstOrDefault(x => x.GroupId == workspaceId && x.ReportId == reportId);
        public async Task<ReportDetail> GetReportForUser(Guid workspaceId, Guid reportId) => (await GetReportDetailsForUser()).FirstOrDefault(x => x.GroupId == workspaceId && x.ReportId == reportId);

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