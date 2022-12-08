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

        //Route matches route in program.cs 
        [Route("/Home/HandleError/{code:int}")]
        public IActionResult HandleError(int code)
        {
            var customError = new CustomError();
            customError.code = code;
            if (code == 404)
            {
                customError.message = "Oops . . .Page not found.";
            }
            else
            {
                customError.message = "Sorry something went wrong.";
            }

            return View("~/Views/Shared/CustomError.cshtml",customError);
        }
    }
}