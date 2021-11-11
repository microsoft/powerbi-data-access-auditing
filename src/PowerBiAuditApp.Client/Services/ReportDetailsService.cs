using Azure.Data.Tables;
using Microsoft.Extensions.Caching.Memory;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services;

public class ReportDetailsService : IReportDetailsService
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly IMemoryCache _memoryCache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private const string CacheKey = nameof(ReportDetail);
    private const int CacheTime = 30;

    public ReportDetailsService(TableServiceClient tableServiceClient, IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
    {
        _tableServiceClient = tableServiceClient;
        _memoryCache = memoryCache;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<IList<ReportDetail>> GetReportDetails() => RetrieveReportDetails();
    public async Task<IList<ReportDetail>> GetReportDetailsForUser()
    {
        var userGroups = _httpContextAccessor.HttpContext?.User.Claims
            .Where(x => x.Type == "groups")
            .Select(x => new Guid(x.Value))
            .ToArray() ?? Array.Empty<Guid>();

        return (await RetrieveReportDetails()).Where(x => x.AadGroups.Any(a => userGroups.Contains(a))).ToList();
    }

    public async Task<ReportDetail?> GetReportDetail(Guid workspaceId, Guid reportId) => (await GetReportDetails()).FirstOrDefault(x => x.GroupId == workspaceId && x.ReportId == reportId);
    public async Task<ReportDetail?> GetReportForUser(Guid workspaceId, Guid reportId) => (await GetReportDetailsForUser()).FirstOrDefault(x => x.GroupId == workspaceId && x.ReportId == reportId);

    private async Task<IList<ReportDetail>> RetrieveReportDetails() =>
        await
            _memoryCache.GetOrCreateAsync(CacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheTime);

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