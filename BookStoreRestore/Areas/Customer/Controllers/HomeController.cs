using System.Diagnostics;
using System.Security.Claims;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreRestore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        // log information for debugging feedback
        // dotnet ef migrations add yourMigrationName
        // and to update database is:
        // dotnet ef database update
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        // MVC
        // Controller receives the request
        // fetch the data from the Model and manipulate
        // Controller renders the view along with the data

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            // user is logged in
            if (claims != null)
            {
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, 
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value).Count());
            }
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(
                includeProperties: "Category"
            );
            return View(products);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId &&
            u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                //shopping cart exists
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                //add cart record
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, 
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            TempData["success"] = "Cart updated successfully";




            return RedirectToAction(nameof(Index));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                }
            );
        }

        private string GetUserID()
        {
            // for shopping each user has his cart so we need to populate it
            // getting userId now
            // (ClaimTypes.NameIdentifier).Value this line gets user id that is stored in this string "(ClaimTypes.NameIdentifier"
            // all that is done by .NET
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}
