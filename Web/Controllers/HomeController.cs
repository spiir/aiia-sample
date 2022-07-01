using System.Diagnostics;
using Aiia.Sample.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Aiia.Sample.Controllers;

public class HomeController : Controller
{
    private readonly bool _isDevelopment;

    public HomeController(IHostEnvironment environment)
    {
        _isDevelopment = environment.IsDevelopment();
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var error = HttpContext
            .Features
            .Get<IExceptionHandlerFeature>();

        if(_isDevelopment)
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorMessage = error?.Error?.Message, ErrorStackTrace = error?.Error?.StackTrace });
        else
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Index()
    {
        return View();
    }
}