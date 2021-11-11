using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json;
using PowerBiAuditApp.Processor.Extensions;
using PowerBiAuditApp.Processor.Models.PowerBi;
using PowerBiAuditApp.Services;

namespace PowerBiAuditApp.Processor;

public class PowerBiActivityEventProcessor
{
    private readonly TimeSpan _maxSyncTimespan = new(8, 30, 0); //30 minutes
    private readonly TimeSpan _oldestRecordToSync = new(5, 0, 0, 0); //5 days
    private readonly IPowerBiReportService _powerBiReportService;

    private readonly TableServiceClient
        _tableServiceClient; //ToDo: this should be moved over to a service binding if the table binding becomes available in "Microsoft.Azure.WebJobs.Extensions.Storage" v5

    public PowerBiActivityEventProcessor(IPowerBiReportService powerBiReportService, TableServiceClient tableServiceClient)
    {
        _powerBiReportService = powerBiReportService;
        _tableServiceClient = tableServiceClient;
    }

    [FunctionName(nameof(PowerBiActivityEventProcessor))]
    public async Task Run(
        [TimerTrigger("0 */30 * * * *")] TimerInfo myTimer,
        [Blob("activity-events", FileAccess.Read, Connection = "StorageAccountBlobEndpoint")] BlobContainerClient blobContainerClient,
        ILogger log)
    {
        log.LogInformation("Starting Sync of Activity Events");

        await blobContainerClient.CreateIfNotExistsAsync();

        var processedTableClient = _tableServiceClient.GetTableClient(nameof(PbiProcessRunTable));
        await processedTableClient.CreateIfNotExistsAsync();

        var lastProcessed = await processedTableClient.QueryAsync<PbiProcessRunTable>(x => x.PartitionKey == nameof(PowerBiActivityEventProcessor)).FirstOrDefaultAsync();

        var lastProcessedDate = await SaveActivityEvents(blobContainerClient, lastProcessed?.LastProcessedDate);

        lastProcessed ??= new PbiProcessRunTable {
            PartitionKey = nameof(PowerBiActivityEventProcessor),
            RowKey = nameof(PowerBiActivityEventProcessor)
        };
        lastProcessed.LastProcessedDate = lastProcessedDate;
        await processedTableClient.UpsertEntityAsync(lastProcessed, TableUpdateMode.Replace);

        log.LogInformation("Finished Sync of Activity Events");
    }


    private async Task<DateTimeOffset> SaveActivityEvents(BlobContainerClient blobContainerClient, DateTimeOffset? date)
    {
        var startDate = date ?? DateTimeOffset.UtcNow.Subtract(_oldestRecordToSync);
        if (startDate.UtcDateTime == startDate.UtcDateTime.Date.AddDays(1).AddMilliseconds(-1))
            startDate = startDate.UtcDateTime.Date.AddDays(1);

        var endDate = new[] { startDate.Add(_maxSyncTimespan), DateTimeOffset.UtcNow }.Max();
        if (endDate.UtcDateTime.Date != startDate.UtcDateTime.Date) // start and end dates must be in the same utc day
            endDate = startDate.UtcDateTime.Date.AddDays(1).AddMilliseconds(-1);

        var activityEvents = await _powerBiReportService.GetActivityEvents(startDate, endDate);
        await SaveActivityEvents(blobContainerClient, activityEvents);
        while (activityEvents is not null && activityEvents.ActivityEventEntities.Count > 0)
        {
            await SaveActivityEvents(blobContainerClient, activityEvents);
            activityEvents = await _powerBiReportService.GetActivityEvents(activityEvents);
        }
        return endDate;
    }

    private async Task SaveActivityEvents(BlobContainerClient blobContainerClient, ActivityEventResponse activityEvents)
    {
        async Task<Response<BlobContentInfo>> UploadAsync(dynamic activityEvent)
        {
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activityEvent)));
            return await blobContainerClient.GetBlobClient($"{activityEvent.Id}.json").UploadAsync(stream, true);
        }

        var tasks = new List<Task<Response<BlobContentInfo>>>();
        foreach (dynamic activityEvent in activityEvents.ActivityEventEntities)
        {
            tasks.Add(UploadAsync(activityEvent));
        }

        await Task.WhenAll(tasks);
    }
}