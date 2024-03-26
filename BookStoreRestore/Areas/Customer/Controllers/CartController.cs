using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreRestore.Areas.Customer.Controllers
{
    [Area("Customer")]
    // user need to be logged in to reach this controller
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM;

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            ShoppingCartVM shoppingCartVM =
                new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                        u => u.ApplicationUserId == GetUserID(),
                        includeProperties: "Product"
                    )
                };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        [HttpGet]
        public IActionResult Plus(int? cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int? cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Delete(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int? cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);

            _unitOfWork.ShoppingCart.Delete(cartFromDb);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            return View();
        }

        /*
        PRIVATE HELPERS SECTION
        */
        private string GetUserID()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        private static double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else if (shoppingCart.Count >= 51 && shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }
    }
}
