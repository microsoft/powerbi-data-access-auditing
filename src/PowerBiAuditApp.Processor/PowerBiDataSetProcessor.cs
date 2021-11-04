using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.PowerBI.Api.Models;
using PowerBiAuditApp.Models;
using PowerBiAuditApp.Processor.Extensions;
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
            var groups = await context.CallActivityAsync<IList<Group>>($"{nameof(PowerBiDataSetProcessor)}_{nameof(GetAndSyncGroups)}", null);

            var tasks = new List<Task>();
            foreach (var group in groups)
            {
                tasks.Add(context.CallActivityAsync($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncReports)}", group));
                tasks.Add(context.CallActivityAsync($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncDataSets)}", group));
                tasks.Add(context.CallActivityAsync($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncDashboards)}", group));
                tasks.Add(context.CallActivityAsync($"{nameof(PowerBiDataSetProcessor)}_{nameof(SyncDataFlows)}", group));
            }
            await Task.WhenAll(tasks);


            await context.CallActivityAsync($"{nameof(PowerBiDataSetProcessor)}_{nameof(UpdateReportSettings)}", null);
        }



        [FunctionName($"{nameof(PowerBiDataSetProcessor)}_{nameof(GetAndSyncGroups)}")]
        public async Task<IList<Group>> GetAndSyncGroups([ActivityTrigger] IDurableActivityContext ctx, ILogger log)
        {
            log.LogInformation("Starting Sync of Groups");
            var now = DateTimeOffset.UtcNow;
            var groups = await _powerBiReportService.GetGroups();

            var tableClient = await GetTableClient<PbiGroupTable>();

            var tasks = new List<Task<Response>>();
            foreach (var group in groups)
            {
                tasks.Add(tableClient.UpsertEntityAsync(new PbiGroupTable(group), TableUpdateMode.Replace));
            }
            await Task.WhenAll(tasks);

            var reportIds = groups.Select(x => x.Id).ToArray();
            var query = tableClient.QueryAsync<PbiReportTable>(x => x.Timestamp < now);

            tasks = new List<Task<Response>>();
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

            var tableClient = await GetTableClient<PbiReportTable>();

            var tasks = new List<Task<Response>>();
            foreach (var report in reports)
            {
                tasks.Add(tableClient.UpsertEntityAsync(new PbiReportTable(report, group.Id), TableUpdateMode.Replace));
            }
            await Task.WhenAll(tasks);

            var reportIds = reports.Select(x => x.Id).ToArray();
            var query = tableClient.QueryAsync<PbiReportTable>(x => x.PartitionKey == group.Id.ToString() && x.Timestamp < now);

            tasks = new List<Task<Response>>();
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

            var tableClient = await GetTableClient<PbiDataSetTable>();

            var tasks = new List<Task<Response>>();
            foreach (var dataSet in dataSets)
            {
                tasks.Add(tableClient.UpsertEntityAsync(new PbiDataSetTable(dataSet, group.Id), TableUpdateMode.Replace));
            }
            await Task.WhenAll(tasks);

            var reportIds = dataSets.Select(x => x.Id).ToArray();
            var query = tableClient.QueryAsync<PbiDataSetTable>(x => x.PartitionKey == group.Id.ToString() && x.Timestamp < now);

            tasks = new List<Task<Response>>();
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

            var tableClient = await GetTableClient<PbiDashboardTable>();

            var tasks = new List<Task<Response>>();
            foreach (var dashboard in dashboards)
            {
                tasks.Add(tableClient.UpsertEntityAsync(new PbiDashboardTable(dashboard, group.Id), TableUpdateMode.Replace));
            }
            await Task.WhenAll(tasks);

            var reportIds = dashboards.Select(x => x.Id).ToArray();
            var query = tableClient.QueryAsync<PbiDashboardTable>(x => x.PartitionKey == group.Id.ToString() && x.Timestamp < now);

            tasks = new List<Task<Response>>();
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

            var tableClient = await GetTableClient<PbiDataFlowTable>();

            var tasks = new List<Task<Response>>();
            foreach (var dataFlow in dataFlows)
            {
                tasks.Add(tableClient.UpsertEntityAsync(new PbiDataFlowTable(dataFlow, group.Id), TableUpdateMode.Replace));
            }
            await Task.WhenAll(tasks);

            var reportIds = dataFlows.Select(x => x.ObjectId).ToArray();
            var query = tableClient
                .QueryAsync<PbiDataFlowTable>(x => x.PartitionKey == group.Id.ToString() && x.Timestamp < now);

            tasks = new List<Task<Response>>();
            await foreach (var pbiDataFlowTable in query)
            {
                if (reportIds.Contains(pbiDataFlowTable.ObjectId))
                    tasks.Add(tableClient.DeleteEntityAsync(pbiDataFlowTable.PartitionKey, pbiDataFlowTable.RowKey));
            }
            await Task.WhenAll(tasks);
            log.LogInformation("Finished Sync of Data Flows for Groups {groupName}", group.Name);
        }

        [FunctionName($"{nameof(PowerBiDataSetProcessor)}_{nameof(UpdateReportSettings)}")]
        public async Task UpdateReportSettings([ActivityTrigger] IDurableActivityContext ctx, ILogger log)
        {
            log.LogInformation("Starting update of Frontend report settings");

            var groupTableClient = await GetTableClient<PbiGroupTable>();
            var pbiGroups = await groupTableClient.QueryAsync<PbiGroupTable>().ToDictionaryAsync(x => x.Id);


            var reportTableClient = await GetTableClient<PbiReportTable>();
            var pbiReports = await reportTableClient.QueryAsync<PbiReportTable>().ToListAsync();

            var reportDetailsTableClient = await GetTableClient<ReportDetail>();
            var reportDetails = await reportDetailsTableClient.QueryAsync<ReportDetail>().ToDictionaryAsync(x => x.ReportId);

            var tasks = new List<Task<Response>>();
            foreach (var pbiReport in pbiReports)
            {
                if (!reportDetails.TryGetValue(pbiReport.Id, out var reportDetail))
                {
                    reportDetail = new ReportDetail {
                        ReportId = pbiReport.Id,
                        Enabled = false,
                        DisplayLevel = 1,
                        Roles = Array.Empty<string>(),
                        Deleted = false
                    };
                }

                if (reportDetail.GroupId != pbiReport.GroupId)
                {
                    reportDetail.GroupId = pbiReport.GroupId;
                    if (!pbiGroups.TryGetValue(pbiReport.GroupId, out var pbiReportGroup))
                        throw new ArgumentException($"Group doesn't seem to exist for report {reportDetail.Name}");
                    reportDetail.GroupName = pbiReportGroup.Name;
                }

                reportDetail.Name = pbiReport.Name;
                reportDetail.Description = pbiReport.Description;
                reportDetail.ReportType = pbiReport.ReportType;
                tasks.Add(reportDetailsTableClient.UpsertEntityAsync(reportDetail, TableUpdateMode.Replace));
            }

            // Delete missing tasks
            var reportIds = pbiReports.Select(x => x.Id).ToList();
            foreach (var (_, reportDetail) in reportDetails.Where(x => reportIds.Contains(x.Key)))
            {
                reportDetail.Deleted = true;
                tasks.Add(reportDetailsTableClient.UpdateEntityAsync(reportDetail, ETag.All));
            }


            await Task.WhenAll(tasks);
            log.LogInformation("Finished update of Frontend report settings");
        }

        private async Task<TableClient> GetTableClient<T>() where T : ITableEntity
        {
            var tableClient = _tableServiceClient.GetTableClient(typeof(T).Name);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }
    }
}