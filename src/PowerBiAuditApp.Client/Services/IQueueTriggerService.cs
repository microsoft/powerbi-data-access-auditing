using System.Threading;
using System.Threading.Tasks;

namespace PowerBiAuditApp.Client.Services
{
    public interface IQueueTriggerService
    {
        Task SendQueueMessage(string message, CancellationToken cancellationToken);
    }
}
