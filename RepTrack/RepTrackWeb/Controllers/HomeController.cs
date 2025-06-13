using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RepTrackWeb.Models;

namespace RepTrackWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string? message = null)
        {
            var errorViewModel = new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                Message = message,
                ShowDetails = _env.IsDevelopment()
            };

            if (!string.IsNullOrEmpty(message))
            {
                _logger.LogError("Error page accessed with message: {Message}", message);
            }

            return View(errorViewModel);
        }
    }
}
