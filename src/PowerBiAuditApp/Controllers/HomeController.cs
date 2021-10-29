using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PowerBiAuditApp.Models;
using PowerBiAuditApp.Services;

namespace PowerBiAuditApp.Controllers;

public class HomeController : Controller
{
    private readonly IReportDetailsService _reportDetailsService;

    public HomeController(IReportDetailsService reportDetailsService)
    {
        _reportDetailsService = reportDetailsService;
    }

    public IActionResult Index()
    {
        var model = new HomeViewModel {
            User = HttpContext.User.Identity?.Name,
            Reports = _reportDetailsService.GetReportDetails()
        };

        return View(model);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}