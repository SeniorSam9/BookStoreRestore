using System.Security.Claims;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace BookStoreRestore.Areas.Customer.Controllers
{
    [Area("Customer")]
    // user need to be logged in to reach this controller
    [Authorize]
    public class CartController : Controller
    {
        private const int REGULAR_CUSTOMER = 0;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            ShoppingCartVM shoppingCartVM =
                new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                        u => u.ApplicationUserId == GetUserID(),
                        includeProperties: "Product"
                    ),
                    OrderHeader = new()
                };
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == GetUserID(),
                    includeProperties: "Product"
                ),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u =>
                u.Id == GetUserID()
            );

            // populating data

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;

            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM
                .OrderHeader
                .ApplicationUser
                .PhoneNumber;

            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM
                .OrderHeader
                .ApplicationUser
                .StreetAddress;

            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;

            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;

            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM
                .OrderHeader
                .ApplicationUser
                .PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        // dont have to recieve any obj as param
        // we already used [BindProperty]
        public IActionResult SummaryPOST()
        {
            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == GetUserID(),
                includeProperties: "Product"
            );
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = GetUserID();

            /*
            OrderHeader
            */
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            // get the user to know his type to set the right status correspondingly
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u =>
                u.Id == GetUserID()
            );

            if (applicationUser.CompanyId.GetValueOrDefault() == REGULAR_CUSTOMER)
            {
                //it is a regular customer
                ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusPending;
            }
            else
            {
                //it is a company user
                ShoppingCartVM.OrderHeader.PaymentStatus =
                    StaticDetails.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusApproved;
            }
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);

            /*
            OrderDetails
            */
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail =
                    new()
                    {
                        ProductId = cart.ProductId,
                        OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                        Price = cart.Price,
                        Count = cart.Count
                    };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == REGULAR_CUSTOMER)
            {
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl =
                        domain
                        + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                // get all items
                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentID(
                    ShoppingCartVM.OrderHeader.Id,
                    session.Id,
                    session.PaymentIntentId
                );
                _unitOfWork.Save();
                // redirecting to another url
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(
                nameof(OrderConfirmation),
                new { orderId = ShoppingCartVM.OrderHeader.Id }
            );
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(
                u => u.Id == orderId,
                includeProperties: "ApplicationUser"
            );
            if (orderHeader.PaymentStatus != StaticDetails.PaymentStatusDelayedPayment)
            {
                //this is an order by customer

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(
                        orderId,
                        session.Id,
                        session.PaymentIntentId
                    );
                    _unitOfWork.OrderHeader.UpdateStatus(
                        orderId,
                        StaticDetails.StatusApproved,
                        StaticDetails.PaymentStatusApproved
                    );
                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear();
            }

            _emailSender.SendEmailAsync(
                orderHeader.ApplicationUser.Email,
                "New Order - Bulky Book",
                $"<p>New Order Created - {orderHeader.Id}</p>"
            );

            List<ShoppingCart> shoppingCarts = _unitOfWork
                .ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId)
                .ToList();

            _unitOfWork.ShoppingCart.DeleteRange(shoppingCarts);
            _unitOfWork.Save();

            return View(orderId);
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
