using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace PowerBiAuditApp.Client.Services
{
    public class GraphService : IGraphService
    {
        private readonly GraphServiceClient _graphServiceClient;

        public GraphService(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public async Task<Dictionary<string, Guid>> GetGroupIds(params string[] groupNames)
        {
            var result = new Dictionary<string, Guid>();
            if (groupNames.Length == 0) return result;

            var filter = groupNames.Distinct().Select(groupName => $"startswith(displayName, '{groupName}')");

            var groups = await _graphServiceClient.Groups.Request().Filter(string.Join(" OR ", filter)).Select(x => new { x.DisplayName, x.Id }).GetAsync();

            //var groups = await _graphServiceClient.Groups.Request().GetAsync();
            do
            {
                foreach (var group in groups)
                {
                    result.Add(group.DisplayName, new Guid(group.Id));
                }
            }
            while (groups.NextPageRequest != null && (groups = await groups.NextPageRequest.GetAsync()).Count > 0);

            return result;
        }

        public Task EnsureRequiredScopes() => _graphServiceClient.Me.Request().GetAsync();
    }
}