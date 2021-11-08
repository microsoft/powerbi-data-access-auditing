using Azure.Data.Tables;
using Microsoft.Extensions.Caching.Memory;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services;

public class ReportDetailsService : IReportDetailsService
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly IMemoryCache _memoryCache;
    private const string CacheKey = nameof(ReportDetail);
    private const int CacheTime = 30;

    public ReportDetailsService(TableServiceClient tableServiceClient, IMemoryCache memoryCache)
    {
        _tableServiceClient = tableServiceClient;
        _memoryCache = memoryCache;
    }

    public Task<IList<ReportDetail>> GetReportDetails() => RetrieveReportDetails();

    public async Task<ReportDetail?> GetReportDetail(Guid workspaceId, Guid reportId) => (await RetrieveReportDetails()).FirstOrDefault(x => x.GroupId == workspaceId && x.ReportId == reportId);

    private async Task<IList<ReportDetail>> RetrieveReportDetails() =>
        await
            _memoryCache.GetOrCreateAsync(CacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(CacheTime);

                var reportDetails = new List<ReportDetail>();
                var tableClient = _tableServiceClient.GetTableClient(nameof(ReportDetail));
                await tableClient.CreateIfNotExistsAsync();
                await foreach (var reportDetail in tableClient.QueryAsync<ReportDetail>())
                {
                    reportDetails.Add(reportDetail);
                }
                return reportDetails;
            });
}