using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PowerBiAuditApp.Client.Services;

namespace PowerBiAuditApp.Client.Controllers
{
    public class AdminController : Controller
    {
        private readonly IQueueTriggerService _queueTriggerService;

        public AdminController(IQueueTriggerService queueTriggerService)
        {
            _queueTriggerService = queueTriggerService;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> RefreshReports()
        {
            // Send message to trigger queue & redirect back to admin page
            var queueClient = await _queueTriggerService.GetTriggerQueue();

            var filename = $"{Guid.NewGuid()} {DateTime.UtcNow:yyyy-MM-dd hh-mm-ss}.json";

            await _queueTriggerService.SendQueueMessage(queueClient, filename);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CreateTriggerQueue()
        {
            await _queueTriggerService.GetTriggerQueue();

            return RedirectToAction("index");
        }
    }
}
