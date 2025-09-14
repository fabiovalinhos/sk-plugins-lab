using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

using Chat.WebBlazorServer.Models;

namespace Chat.WebBlazorServer.Controllers
{
    public class HomeController(ILogger<HomeController> logger) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}