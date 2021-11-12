using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.PowerBI.Api.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using PowerBiAuditApp.Processor.Extensions;
using PowerBiAuditApp.Processor.Models.PowerBi;
using PowerBiAuditApp.Services;

namespace PowerBiAuditApp.Processor
{
    public class PowerBiActivityEventProcessor
    {
        private readonly TimeSpan _maxSyncTimespan = new(8, 30, 0); //30 minutes
        private readonly TimeSpan _oldestRecordToSync = new(5, 0, 0, 0); //5 days
        private readonly IPowerBiReportService _powerBiReportService;

        public PowerBiActivityEventProcessor(IPowerBiReportService powerBiReportService)
        {
            _powerBiReportService = powerBiReportService;
        }

        [FunctionName(nameof(PowerBiActivityEventProcessor))]
        public async Task Run(
            [TimerTrigger("0 */30 * * * *")] TimerInfo myTimer,
            [Blob("activity-events", Connection = "StorageAccountBlobEndpoint")] CloudBlobContainer cloudBlobContainer,
            [Table(nameof(PbiProcessRunTable), Connection = "StorageAccountTableEndpoint")] CloudTable processRunTable,
            ILogger log)
        {
            log.LogInformation("Starting Sync of Activity Events");

            await cloudBlobContainer.CreateIfNotExistsAsync();
            await processRunTable.CreateIfNotExistsAsync();

            var lastProcessed = await processRunTable.GetAsync<PbiProcessRunTable>(nameof(PowerBiActivityEventProcessor), nameof(PowerBiActivityEventProcessor));

            var lastProcessedDate = await SaveActivityEvents(cloudBlobContainer, lastProcessed?.LastProcessedDate);

            lastProcessed ??= new PbiProcessRunTable {
                PartitionKey = nameof(PowerBiActivityEventProcessor),
                RowKey = nameof(PowerBiActivityEventProcessor)
            };
            lastProcessed.LastProcessedDate = lastProcessedDate;
            await processRunTable.UpsertEntityAsync(lastProcessed);

            log.LogInformation("Finished Sync of Activity Events");
        }

        private async Task<DateTimeOffset> SaveActivityEvents(CloudBlobContainer cloudBlobContainer, DateTimeOffset? date)
        {
            var startDate = date ?? DateTimeOffset.UtcNow.Subtract(_oldestRecordToSync);
            if (startDate.UtcDateTime == startDate.UtcDateTime.Date.AddDays(1).AddMilliseconds(-1))
                startDate = startDate.UtcDateTime.Date.AddDays(1);

            var endDate = new[] { startDate.Add(_maxSyncTimespan), DateTimeOffset.UtcNow }.Max();
            if (endDate.UtcDateTime.Date != startDate.UtcDateTime.Date) // start and end dates must be in the same utc day
                endDate = startDate.UtcDateTime.Date.AddDays(1).AddMilliseconds(-1);

            var activityEvents = await _powerBiReportService.GetActivityEvents(startDate, endDate);
            await SaveActivityEvents(cloudBlobContainer, activityEvents);
            while (activityEvents is not null && activityEvents.ActivityEventEntities.Count > 0)
            {
                await SaveActivityEvents(cloudBlobContainer, activityEvents);
                activityEvents = await _powerBiReportService.GetActivityEvents(activityEvents);
            }
            return endDate;
        }

        private async Task SaveActivityEvents(CloudBlobContainer cloudBlobContainer, ActivityEventResponse activityEvents)
        {
            async Task UploadAsync(dynamic activityEvent)
            {
                var reference = cloudBlobContainer.GetBlockBlobReference($"{activityEvent.Id}.json");
                reference.Properties.ContentType = "application/json";
                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activityEvent));
                await reference.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
            }

            var tasks = activityEvents.ActivityEventEntities.Select(UploadAsync).ToArray();
            await Task.WhenAll(tasks);
        }
    }
}