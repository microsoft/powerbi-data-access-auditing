using Microsoft.AspNetCore.Mvc;
using PowerBiAuditApp.Models;
using PowerBiAuditApp.Services;
using System.Diagnostics;

namespace PowerBiAuditApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IReportDetailsService _reportDetailsService;

    public HomeController(ILogger<HomeController> logger, IReportDetailsService reportDetailsService)
    {
        _logger = logger;
        _reportDetailsService = reportDetailsService;
    }

    public IActionResult Index()
    {
        ViewData.Add("User", HttpContext.User.Identity?.Name);
        ViewData.Add("Reports", _reportDetailsService.GetReportDetails());

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}