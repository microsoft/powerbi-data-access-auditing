using Azure.Storage.Queues;

namespace PowerBiAuditApp.Client.Services;

public interface IQueueTriggerService
{
    Task<QueueClient> GetTriggerQueue(CancellationToken cancellationToken = default);
    Task SendQueueMessage(QueueClient queueClient, string message, CancellationToken cancellationToken = default);
}
