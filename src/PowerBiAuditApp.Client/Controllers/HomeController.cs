using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Client.Services;

namespace PowerBiAuditApp.Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly IReportDetailsService _reportDetailsService;

        public HomeController(IReportDetailsService reportDetailsService)
        {
            _reportDetailsService = reportDetailsService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel {
                User = HttpContext.User.Identity?.Name,
                Reports = (await _reportDetailsService.GetReportDetailsForUser())
                    .OrderBy(x => x.GroupName)
                    .ThenBy(x => x.Name)
                    .GroupBy(x => x.GroupName)
                    .ToDictionary(x => x.Key, x => x.ToArray())

            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}