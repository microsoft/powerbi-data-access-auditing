using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PowerBiAuditApp.Client.Models;
using PowerBiAuditApp.Client.Services;

namespace PowerBiAuditApp.Client.Controllers;

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
            Reports = await _reportDetailsService.GetReportDetails()
        };

        return View(model);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}