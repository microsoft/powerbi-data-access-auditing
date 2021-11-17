using Azure.Storage.Queues;

namespace PowerBiAuditApp.Client.Services;

public interface IQueueTriggerService
{
    Task SendQueueMessage(string message, CancellationToken cancellationToken);
}
