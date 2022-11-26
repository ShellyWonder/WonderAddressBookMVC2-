using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WonderAddressBookMVC_.Models;

namespace WonderAddressBookMVC_.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Landing()
        {
            return View();
        }
    }
}