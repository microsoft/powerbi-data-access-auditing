using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;

namespace PowerBiAuditApp.Services;

public class PowerBiReportService : IPowerBiReportService
{
    private readonly IPowerBiTokenProvider _tokenProvider;
    private const string UrlPowerBiServiceApiRoot = "https://api.powerbi.com";

    public PowerBiReportService(IPowerBiTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    /// <summary>
    /// Get Power BI client
    /// </summary>
    /// <returns>Power BI client object</returns>
    private PowerBIClient GetPowerBiClient()
    {
        var tokenCredentials = new TokenCredentials(_tokenProvider.GetAccessToken(), "Bearer");
        return new PowerBIClient(new Uri(UrlPowerBiServiceApiRoot), tokenCredentials);
    }

    public async Task<IList<Group>> GetGroups()
    {
        var pbiClient = GetPowerBiClient();
        return (await pbiClient.Groups.GetGroupsAsync()).Value;
    }

    public async Task<IList<Report>> GetReports(Guid groupId)
    {
        var pbiClient = GetPowerBiClient();
        return (await pbiClient.Reports.GetReportsAsync(groupId)).Value;
    }

    public async Task<IList<Dataset>> GetDataSets(Guid groupId)
    {
        var pbiClient = GetPowerBiClient();
        return (await pbiClient.Datasets.GetDatasetsAsync(groupId)).Value;
    }

    public async Task<IList<Dashboard>> GetDashboards(Guid groupId)
    {
        var pbiClient = GetPowerBiClient();
        return (await pbiClient.Dashboards.GetDashboardsAsync(groupId)).Value;
    }

    public async Task<IList<Dataflow>> GetDataFlows(Guid groupId)
    {
        var pbiClient = GetPowerBiClient();
        return (await pbiClient.Dataflows.GetDataflowsAsync(groupId)).Value;
    }
}