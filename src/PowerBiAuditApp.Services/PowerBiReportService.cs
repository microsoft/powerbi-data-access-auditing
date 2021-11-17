using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;

namespace PowerBiAuditApp.Services
{
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
        private PowerBIClient GetPowerBiClient(string serviceRoot = UrlPowerBiServiceApiRoot)
        {
            var tokenCredentials = new TokenCredentials(_tokenProvider.GetAccessToken(), "Bearer");
            return new PowerBIClient(new Uri(serviceRoot), tokenCredentials);
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

        public async Task<ActivityEventResponse> GetActivityEvents(DateTimeOffset start, DateTimeOffset end)
        {
            var pbiClient = GetPowerBiClient();
            return await pbiClient.Admin.GetActivityEventsAsync($"'{start.UtcDateTime:yyyy-MM-ddTHH:mm:ssK}'", $"'{end.UtcDateTime:yyyy-MM-ddTHH:mm:ssK}'");
        }

        public async Task<ActivityEventResponse> GetActivityEvents(ActivityEventResponse activityEventResponse)
        {
            if (activityEventResponse is null) return null;
            var uri = new Uri(activityEventResponse.ContinuationUri);
            var pbiClient = GetPowerBiClient($"{uri.Scheme}://{uri.Host}");
            return await pbiClient.Admin.GetActivityEventsAsync(continuationToken: $"'{WebUtility.UrlDecode(activityEventResponse.ContinuationToken)}'");
        }
    }
}