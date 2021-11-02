using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.PowerBI.Api.Models;
using PowerBiAuditApp.Processor.Models.PowerBi;
using PowerBiAuditApp.Services;

namespace PowerBiAuditApp.Processor
{
    public class PowerBiDataSetProcessor
    {
        private readonly IPowerBiReportService _powerBiReportService;
        private readonly TableServiceClient _tableServiceClient; //ToDo this should be moved over to a service binding if the table binding becomes available in "Microsoft.Azure.WebJobs.Extensions.Storage" v5

        public PowerBiDataSetProcessor(IPowerBiReportService powerBiReportService, TableServiceClient tableServiceClient)
        {
            _powerBiReportService = powerBiReportService;
            _tableServiceClient = tableServiceClient;
        }


        [FunctionName($"{nameof(PowerBiDataSetProcessor)}_{nameof(HttpStart)}")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            var instanceId = await starter.StartNewAsync(nameof(PowerBiDataSetProcessor));

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName($"{nameof(PowerBiDataSetProcessor)}_{nameof(TimerStart)}")]
        public static async Task TimerStart(
            [TimerTrigger("0 0 2 * * *")] TimerInfo myTimer,// At 2:00am
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            var instanceId = await starter.StartNewAsync(nameof(PowerBiDataSetProcessor));

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }


        [FunctionName(nameof(PowerBiDataSetProcessor))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var groups = await context.CallActivityAsync<IList<Group>>($"{nameof(PowerBiDataSetProcessor)}_{nameof(GetGroups)}", null);

            var tasks = new List<Task>();
            foreach (var group in groups)
            {
                tasks.Add(context.CallActivityAsync($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncReports)}", group));
                tasks.Add(context.CallActivityAsync($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncDataSets)}", group));
                tasks.Add(context.CallActivityAsync($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncDashboards)}", group));
                tasks.Add(context.CallActivityAsync($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncDataFlows)}", group));
            }
            await Task.WhenAll(tasks);
        }



        [FunctionName($"{nameof(PowerBiDataSetProcessor)}_{nameof(GetGroups)}")]
        public async Task<IList<Group>> GetGroups([ActivityTrigger] IDurableActivityContext ctx, ILogger log)
        {
            log.LogInformation("Starting Sync of Groups");
            var now = DateTimeOffset.UtcNow;
            var groups = await _powerBiReportService.GetGroups();

            var tableClient = _tableServiceClient.GetTableClient(nameof(PbiReportTable));
            await tableClient.CreateIfNotExistsAsync();

            var tasks = new List<Task<Azure.Response>>();
            foreach (var group in groups)
            {
                tasks.Add(tableClient.UpsertEntityAsync(new PbiGroupTable(group), TableUpdateMode.Replace));
            }
            await Task.WhenAll(tasks);

            var reportIds = groups.Select(x => x.Id).ToArray();
            var query = tableClient.QueryAsync<PbiReportTable>(x => x.Timestamp < now);

            tasks = new List<Task<Azure.Response>>();
            await foreach (var pbiReportTable in query)
            {
                if (reportIds.Contains(pbiReportTable.Id))
                    tasks.Add(tableClient.DeleteEntityAsync(pbiReportTable.PartitionKey, pbiReportTable.RowKey));
            }
            await Task.WhenAll(tasks);
            log.LogInformation("Finished Sync of Groups");
            return groups;
        }


        [FunctionName($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncReports)}")]
        public async Task SyncReports([ActivityTrigger] Group group, ILogger log)
        {
            log.LogInformation("Starting Sync of Reports for Groups {groupName}", group.Name);
            var now = DateTimeOffset.UtcNow;
            var reports = await _powerBiReportService.GetReports(group.Id);

            var tableClient = _tableServiceClient.GetTableClient(nameof(PbiReportTable));
            await tableClient.CreateIfNotExistsAsync();

            var tasks = new List<Task<Azure.Response>>();
            foreach (var report in reports)
            {
                tasks.Add(tableClient.UpsertEntityAsync(new PbiReportTable(report, group.Id), TableUpdateMode.Replace));
            }
            await Task.WhenAll(tasks);

            var reportIds = reports.Select(x => x.Id).ToArray();
            var query = tableClient.QueryAsync<PbiReportTable>(x => x.PartitionKey == group.Id.ToString() && x.Timestamp < now);

            tasks = new List<Task<Azure.Response>>();
            await foreach (var pbiGroupTable in query)
            {
                if (reportIds.Contains(pbiGroupTable.Id))
                    tasks.Add(tableClient.DeleteEntityAsync(pbiGroupTable.PartitionKey, pbiGroupTable.RowKey));
            }
            await Task.WhenAll(tasks);
            log.LogInformation("Finished Sync of Reports for Groups {groupName}", group.Name);
        }


        [FunctionName($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncDataSets)}")]
        public async Task SyncDataSets([ActivityTrigger] Group group, ILogger log)
        {
            log.LogInformation("Starting Sync of Data Sets for Groups {groupName}", group.Name);
            var now = DateTimeOffset.UtcNow;
            var dataSets = await _powerBiReportService.GetDataSets(group.Id);

            var tableClient = _tableServiceClient.GetTableClient(nameof(PbiDataSetTable));
            await tableClient.CreateIfNotExistsAsync();

            var tasks = new List<Task<Azure.Response>>();
            foreach (var dataSet in dataSets)
            {
                tasks.Add(tableClient.UpsertEntityAsync(new PbiDataSetTable(dataSet, group.Id), TableUpdateMode.Replace));
            }
            await Task.WhenAll(tasks);

            var reportIds = dataSets.Select(x => x.Id).ToArray();
            var query = tableClient.QueryAsync<PbiDataSetTable>(x => x.PartitionKey == group.Id.ToString() && x.Timestamp < now);

            tasks = new List<Task<Azure.Response>>();
            await foreach (var pbiDataSetTable in query)
            {
                if (reportIds.Contains(pbiDataSetTable.Id))
                    tasks.Add(tableClient.DeleteEntityAsync(pbiDataSetTable.PartitionKey, pbiDataSetTable.RowKey));
            }
            await Task.WhenAll(tasks);
            log.LogInformation("Finished Sync of Data Sets for Groups {groupName}", group.Name);
        }

        [FunctionName($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncDashboards)}")]
        public async Task SyncDashboards([ActivityTrigger] Group group, ILogger log)
        {
            log.LogInformation("Starting Sync of Dashboards for Groups {groupName}", group.Name);
            var now = DateTimeOffset.UtcNow;
            var dashboards = await _powerBiReportService.GetDashboards(group.Id);

            var tableClient = _tableServiceClient.GetTableClient(nameof(PbiDashboardTable));
            await tableClient.CreateIfNotExistsAsync();

            var tasks = new List<Task<Azure.Response>>();
            foreach (var dashboard in dashboards)
            {
                tasks.Add(tableClient.UpsertEntityAsync(new PbiDashboardTable(dashboard, group.Id), TableUpdateMode.Replace));
            }
            await Task.WhenAll(tasks);

            var reportIds = dashboards.Select(x => x.Id).ToArray();
            var query = tableClient.QueryAsync<PbiDashboardTable>(x => x.PartitionKey == group.Id.ToString() && x.Timestamp < now);

            tasks = new List<Task<Azure.Response>>();
            await foreach (var pbiDashboardTable in query)
            {
                if (reportIds.Contains(pbiDashboardTable.Id))
                    tasks.Add(tableClient.DeleteEntityAsync(pbiDashboardTable.PartitionKey, pbiDashboardTable.RowKey));
            }
            await Task.WhenAll(tasks);

            log.LogInformation("Finished Sync of Dashboards for Groups {groupName}", group.Name);
        }

        [FunctionName($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncDataFlows)}")]
        public async Task SyncDataFlows([ActivityTrigger] Group group, ILogger log)
        {
            log.LogInformation("Starting Sync of Data Flows for Groups {groupName}", group.Name);
            var now = DateTimeOffset.UtcNow;
            var dataFlows = await _powerBiReportService.GetDataFlows(group.Id);

            var tableClient = _tableServiceClient.GetTableClient(nameof(PbiDataFlowTable));
            await tableClient.CreateIfNotExistsAsync();

            var tasks = new List<Task<Azure.Response>>();
            foreach (var dataFlow in dataFlows)
            {
                tasks.Add(tableClient.UpsertEntityAsync(new PbiDataFlowTable(dataFlow, group.Id), TableUpdateMode.Replace));
            }
            await Task.WhenAll(tasks);

            var reportIds = dataFlows.Select(x => x.ObjectId).ToArray();
            var query = tableClient.QueryAsync<PbiDataFlowTable>(x => x.PartitionKey == group.Id.ToString() && x.Timestamp < now);

            tasks = new List<Task<Azure.Response>>();
            await foreach (var pbiDataFlowTable in query)
            {
                if (reportIds.Contains(pbiDataFlowTable.ObjectId))
                    tasks.Add(tableClient.DeleteEntityAsync(pbiDataFlowTable.PartitionKey, pbiDataFlowTable.RowKey));
            }
            await Task.WhenAll(tasks);
            log.LogInformation("Finished Sync of Data Flows for Groups {groupName}", group.Name);
        }
    }
}