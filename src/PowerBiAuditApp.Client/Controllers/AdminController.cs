using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using PowerBiAuditApp.Client.Security;
using PowerBiAuditApp.Client.Services;

namespace PowerBiAuditApp.Client.Controllers
{

    [AdministratorAuthorize]
    [AuthorizeForScopes(Scopes = new[] { "Group.Read.All" })]
    [RequiredScope("Group.Read.All")]
    public class AdminController : Controller
    {
        private readonly IQueueTriggerService _queueTriggerService;
        private readonly IGraphService _graphService;

        public AdminController(IQueueTriggerService queueTriggerService, IGraphService graphService)
        {
            _queueTriggerService = queueTriggerService;
            _graphService = graphService;
        }

        public async Task<IActionResult> Index()
        {

            await _graphService.EnsureRequiredScopes();
            // Example graph service call (expect exception and call back if token has expired).
            //var bob = await _graphService.GetGroupIds("City of Bunbury - Projects", "Regis Resources - Projects");
            return View();
        }

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
