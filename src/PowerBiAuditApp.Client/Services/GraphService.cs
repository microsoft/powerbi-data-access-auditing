using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using PowerBiAuditApp.Models;

namespace PowerBiAuditApp.Client.Services
{
    public class GraphService : IGraphService
    {
        private readonly GraphServiceClient _graphServiceClient;

        public GraphService(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public async Task<List<AadGroup>> GetGroupIds(params string[] groupNames)
        {
            var result = new List<AadGroup>();
            if (groupNames.Length == 0) return result;

            var filter = groupNames.Distinct().Select(groupName => $"displayName eq '{groupName}'");

            var groups = await _graphServiceClient.Groups.Request().Filter(string.Join(" OR ", filter)).Select(x => new { x.DisplayName, x.Id }).GetAsync();

            //var groups = await _graphServiceClient.Groups.Request().GetAsync();
            do
            {
                foreach (var group in groups)
                {
                    result.Add(new AadGroup()
                    {
                        Id = new Guid(group.Id),
                        Name = group.DisplayName
                    });
                }
            }
            while (groups.NextPageRequest != null && (groups = await groups.NextPageRequest.GetAsync()).Count > 0);

            return result;
        }

        public async Task<List<AadGroup>> QueryGroups(string groupName)
        {
            var result = new List<AadGroup>();
            if (groupName is null || groupName.Length <= 3) return result;

            var groups = await _graphServiceClient.Groups.Request().Filter($"startswith(displayName, '{groupName}')").Select(x => new { x.DisplayName, x.Id }).GetAsync();

            //var groups = await _graphServiceClient.Groups.Request().GetAsync();
            do
            {
                foreach (var group in groups)
                {
                    result.Add(new AadGroup() {
                        Id = new Guid(group.Id),
                        Name = group.DisplayName
                    });
                }
            }
            while (groups.NextPageRequest != null && (groups = await groups.NextPageRequest.GetAsync()).Count > 0);

            return result;
        }

        public Task EnsureRequiredScopes() => _graphServiceClient.Me.Request().GetAsync();
    }
}