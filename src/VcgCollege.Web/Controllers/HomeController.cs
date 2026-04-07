using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VcgCollege.Web.Authorization;
using VcgCollege.Web.Models;

namespace VcgCollege.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHostEnvironment _env;

    public HomeController(ILogger<HomeController> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true
            && User.IsInRole(RoleNames.Faculty)
            && !User.IsInRole(RoleNames.Administrator))
            return RedirectToAction(nameof(FacultyController.Index), "Faculty");

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        if (feature?.Error != null)
        {
            _logger.LogError(
                feature.Error,
                "Unhandled exception. Path={Path} RequestId={RequestId}",
                feature.Path,
                requestId);
        }
        else
        {
            _logger.LogWarning("Error page shown without exception feature. RequestId={RequestId}", requestId);
        }

        Response.StatusCode = StatusCodes.Status500InternalServerError;

        return View(new ErrorViewModel
        {
            RequestId = requestId,
            ShowTechnicalDetails = _env.IsDevelopment() && feature?.Error != null,
            TechnicalSummary = _env.IsDevelopment() && feature?.Error != null
                ? feature.Error.GetType().Name + ": " + feature.Error.Message
                : null
        });
    }

    public IActionResult NotFound([FromQuery] int statusCode = 404)
    {
        Response.StatusCode = statusCode;
        return View();
    }
}
