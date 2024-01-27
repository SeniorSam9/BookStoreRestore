using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookStoreRestore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        // log information for debugging feedback
        private readonly ILogger<HomeController> _logger;
        // MVC
        // Controller receives the request 
        // fetch the data from the Model and manipulate 
        // Controller renders the view along with the data

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        // actions methods
        public IActionResult Index()
        {
            // how to know which view to render??
            // when no view is specified it will use the view that is same name as the action method
            // again how will it know??
            // it sees the controller name
            // checks the view folder with the same name as the controller
            // if return View("Privacy");
            // it will render privacy page
            // views are replaced with @RenderBody in _layout file
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
}