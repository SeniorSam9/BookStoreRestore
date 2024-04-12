using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreRestore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Admin_Role)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders = _unitOfWork
                .OrderHeader.GetAll(includeProperties: "ApplicationUser")
                .ToList();

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u =>
                        u.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment
                    );
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u =>
                        u.OrderStatus == StaticDetails.StatusInProcess
                    );
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u =>
                        u.OrderStatus == StaticDetails.StatusShipped
                    );
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u =>
                        u.OrderStatus == StaticDetails.StatusApproved
                    );
                    break;
                default:
                    break;
            }

            return Json(new { success = true, data = orderHeaders });
        }
        #endregion
    }
}
