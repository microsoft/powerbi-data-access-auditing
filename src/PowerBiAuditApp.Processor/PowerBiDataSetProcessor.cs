using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.PowerBI.Api.Models;
using Microsoft.WindowsAzure.Storage.Table;
using PowerBiAuditApp.Models;
using PowerBiAuditApp.Processor.Extensions;
using PowerBiAuditApp.Processor.Models.PowerBi;
using PowerBiAuditApp.Services;

namespace PowerBiAuditApp.Processor
{
    public class PowerBiDataSetProcessor
    {
        private readonly IPowerBiReportService _powerBiReportService;

        public PowerBiDataSetProcessor(IPowerBiReportService powerBiReportService)
        {
            _powerBiReportService = powerBiReportService;
        }


        [FunctionName(nameof(PowerBiDataSetProcessor_QueueStart))]
        public static async Task PowerBiDataSetProcessor_QueueStart(
            [QueueTrigger("app-trigger-queue", Connection = "StorageAccountQueueEndpoint")] string name,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            var instanceId = await starter.StartNewAsync(nameof(PowerBiDataSetProcessor));

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }


        [FunctionName(nameof(PowerBiDataSetProcessor_TimerStart))]
        public static async Task PowerBiDataSetProcessor_TimerStart(
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
            var groups = await context.CallActivityAsync<IList<Group>>(nameof(PowerBiDataSetProcessor_GetAndSyncGroups), null);

            var tasks = new List<Task>();
            foreach (var group in groups)
            {
                tasks.Add(context.CallActivityAsync(nameof(PowerBiDataSetProcessor_SyncReports), group));
                tasks.Add(context.CallActivityAsync(nameof(PowerBiDataSetProcessor_SyncDataSets), group));
                tasks.Add(context.CallActivityAsync(nameof(PowerBiDataSetProcessor_SyncDashboards), group));
                tasks.Add(context.CallActivityAsync(nameof(PowerBiDataSetProcessor_SyncDataFlows), group));
            }
            await Task.WhenAll(tasks);


            await context.CallActivityAsync(nameof(PowerBiDataSetProcessor_UpdateReportSettings), null);
        }



        [FunctionName(nameof(PowerBiDataSetProcessor_GetAndSyncGroups))]
        public async Task<IList<Group>> PowerBiDataSetProcessor_GetAndSyncGroups(
            [ActivityTrigger] IDurableActivityContext ctx,
            [Table(nameof(PbiGroupTable), Connection = "StorageAccountTableEndpoint")] CloudTable groupTable,
            ILogger log)
        {
            log.LogInformation("Starting Sync of Groups");
            await groupTable.CreateIfNotExistsAsync();

            var now = DateTimeOffset.UtcNow;
            var groups = await _powerBiReportService.GetGroups();


            var tasks = new List<Task>();
            foreach (var group in groups)
            {
                tasks.Add(groupTable.UpsertEntityAsync(new PbiGroupTable(group)));
            }
            await Task.WhenAll(tasks);

            var reportIds = groups.Select(x => x.Id).ToArray();
            var rowsToDelete = await groupTable.GetEntitiesOlderThanAsync<PbiGroupTable>(now);

            tasks = new List<Task>();
            foreach (var pbiReportTable in rowsToDelete)
            {
                if (reportIds.Contains(pbiReportTable.Id))
                    tasks.Add(groupTable.DeleteEntityAsync(pbiReportTable));
            }
            await Task.WhenAll(tasks);
            log.LogInformation("Finished Sync of Groups");
            return groups;
        }


        [FunctionName(nameof(PowerBiDataSetProcessor_SyncReports))]
        public async Task PowerBiDataSetProcessor_SyncReports(
            [ActivityTrigger] Group group,
            [Table(nameof(PbiReportTable), Connection = "StorageAccountTableEndpoint")] CloudTable reportTable,
            ILogger log)
        {
            log.LogInformation("Starting Sync of Reports for Groups {groupName}", group.Name);
            await reportTable.CreateIfNotExistsAsync();

            var now = DateTimeOffset.UtcNow;
            var reports = await _powerBiReportService.GetReports(group.Id);

            var tasks = new List<Task>();
            foreach (var report in reports)
            {
                tasks.Add(reportTable.UpsertEntityAsync(new PbiReportTable(report, group.Id)));
            }
            await Task.WhenAll(tasks);

            var reportIds = reports.Select(x => x.Id).ToArray();
            var query = await reportTable.GetEntitiesOlderThanAsync<PbiReportTable>(group.Id.ToString(), now);

            tasks = new List<Task>();
            foreach (var pbiGroupTable in query)
            {
                if (reportIds.Contains(pbiGroupTable.Id))
                    tasks.Add(reportTable.DeleteEntityAsync(pbiGroupTable));
            }
            await Task.WhenAll(tasks);
            log.LogInformation("Finished Sync of Reports for Groups {groupName}", group.Name);
        }


        [FunctionName(nameof(PowerBiDataSetProcessor_SyncDataSets))]
        public async Task PowerBiDataSetProcessor_SyncDataSets(
            [ActivityTrigger] Group group,
            [Table(nameof(PbiDataSetTable), Connection = "StorageAccountTableEndpoint")] CloudTable dataSetTable,
            ILogger log)
        {
            log.LogInformation("Starting Sync of Data Sets for Groups {groupName}", group.Name);
            await dataSetTable.CreateIfNotExistsAsync();

            var now = DateTimeOffset.UtcNow;
            var dataSets = await _powerBiReportService.GetDataSets(group.Id);

            var tasks = new List<Task>();
            foreach (var dataSet in dataSets)
            {
                tasks.Add(dataSetTable.UpsertEntityAsync(new PbiDataSetTable(dataSet, group.Id)));
            }
            await Task.WhenAll(tasks);

            var reportIds = dataSets.Select(x => x.Id).ToArray();
            var query = await dataSetTable.GetEntitiesOlderThanAsync<PbiDataSetTable>(group.Id.ToString(), now);

            tasks = new List<Task>();
            foreach (var pbiDataSetTable in query)
            {
                if (reportIds.Contains(pbiDataSetTable.Id))
                    tasks.Add(dataSetTable.DeleteEntityAsync(pbiDataSetTable));
            }
            await Task.WhenAll(tasks);
            log.LogInformation("Finished Sync of Data Sets for Groups {groupName}", group.Name);
        }

        [FunctionName(nameof(PowerBiDataSetProcessor_SyncDashboards))]
        public async Task PowerBiDataSetProcessor_SyncDashboards(
            [ActivityTrigger] Group group,
            [Table(nameof(PbiDashboardTable), Connection = "StorageAccountTableEndpoint")] CloudTable dashboardTable,
            ILogger log)
        {
            log.LogInformation("Starting Sync of Dashboards for Groups {groupName}", group.Name);
            await dashboardTable.CreateIfNotExistsAsync();

            var now = DateTimeOffset.UtcNow;
            var dashboards = await _powerBiReportService.GetDashboards(group.Id);

            var tasks = new List<Task>();
            foreach (var dashboard in dashboards)
            {
                tasks.Add(dashboardTable.UpsertEntityAsync(new PbiDashboardTable(dashboard, group.Id)));
            }
            await Task.WhenAll(tasks);

            var reportIds = dashboards.Select(x => x.Id).ToArray();
            var query = await dashboardTable.GetEntitiesOlderThanAsync<PbiDashboardTable>(group.Id.ToString(), now);

            tasks = new List<Task>();
            foreach (var pbiDashboardTable in query)
            {
                if (reportIds.Contains(pbiDashboardTable.Id))
                    tasks.Add(dashboardTable.DeleteEntityAsync(pbiDashboardTable));
            }
            await Task.WhenAll(tasks);

            log.LogInformation("Finished Sync of Dashboards for Groups {groupName}", group.Name);
        }

        [FunctionName(nameof(PowerBiDataSetProcessor_SyncDataFlows))]
        public async Task PowerBiDataSetProcessor_SyncDataFlows(
            [ActivityTrigger] Group group,
            [Table(nameof(PbiDataFlowTable), Connection = "StorageAccountTableEndpoint")] CloudTable dataFlowTable,
            ILogger log)
        {
            log.LogInformation("Starting Sync of Data Flows for Groups {groupName}", group.Name);
            await dataFlowTable.CreateIfNotExistsAsync();

            var now = DateTimeOffset.UtcNow;
            var dataFlows = await _powerBiReportService.GetDataFlows(group.Id);



            var tasks = new List<Task>();
            foreach (var dataFlow in dataFlows)
            {
                tasks.Add(dataFlowTable.UpsertEntityAsync(new PbiDataFlowTable(dataFlow, group.Id)));
            }
            await Task.WhenAll(tasks);

            var reportIds = dataFlows.Select(x => x.ObjectId).ToArray();
            var query = await dataFlowTable
                .GetEntitiesOlderThanAsync<PbiDataFlowTable>(group.Id.ToString(), now);

            tasks = new List<Task>();
            foreach (var pbiDataFlowTable in query)
            {
                if (reportIds.Contains(pbiDataFlowTable.ObjectId))
                    tasks.Add(dataFlowTable.DeleteEntityAsync(pbiDataFlowTable));
            }
            await Task.WhenAll(tasks);
            log.LogInformation("Finished Sync of Data Flows for Groups {groupName}", group.Name);
        }

        [FunctionName(nameof(PowerBiDataSetProcessor_UpdateReportSettings))]
        public async Task PowerBiDataSetProcessor_UpdateReportSettings(
            [ActivityTrigger] IDurableActivityContext ctx,
            [Table(nameof(PbiGroupTable), Connection = "StorageAccountTableEndpoint")] CloudTable groupTable,
            [Table(nameof(PbiReportTable), Connection = "StorageAccountTableEndpoint")] CloudTable reportTable,
            [Table(nameof(ReportDetail), Connection = "StorageAccountTableEndpoint")] CloudTable reportDetailTable,
            ILogger log)
        {
            log.LogInformation("Starting update of Frontend report settings");

            var pbiGroups = await groupTable.ToDictionaryAsync<PbiGroupTable, Guid>(x => x.Id);

            var pbiReports = await reportTable.ToListAsync<PbiReportTable>();

            var reportDetails = await reportDetailTable.ToDictionaryAsync<ReportDetail, Guid>(x => x.ReportId);

            var tasks = new List<Task>();
            foreach (var pbiReport in pbiReports)
            {
                if (!reportDetails.TryGetValue(pbiReport.Id, out var reportDetail))
                {
                    if (!pbiGroups.TryGetValue(pbiReport.GroupId, out var pbiReportGroup))
                        throw new ArgumentException($"Group doesn't seem to exist for report {reportDetail.Name}");

                    reportDetail = new ReportDetail {
                        ReportId = pbiReport.Id,
                        GroupId = pbiReport.GroupId,
                        GroupName = pbiReportGroup.Name,
                        Enabled = false,
                        DisplayLevel = 1,
                        Roles = Array.Empty<string>(),
                        AadGroups = Array.Empty<Guid>(),
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
                tasks.Add(reportDetailTable.UpsertEntityAsync(reportDetail));
            }

            // Delete missing tasks
            var reportIds = pbiReports.Select(x => x.Id).ToList();
            foreach (var (_, reportDetail) in reportDetails.Where(x => reportIds.Contains(x.Key)))
            {
                reportDetail.Deleted = true;
                tasks.Add(reportDetailTable.UpsertEntityAsync(reportDetail));
            }


            await Task.WhenAll(tasks);
            log.LogInformation("Finished update of Frontend report settings");
        }
    }
}