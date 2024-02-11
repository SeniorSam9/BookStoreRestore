using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStoreRestore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.Product.GetAll().ToList();
            
            return View(products);
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            // we are returning a view with product list
            // but what if we wanted to render the view with more than one list?
            // view (products, categories)?? it is not possible
            // so here comes the feature of selectlistitem
            // and we are projecting (means: all categories object they are parsed into name and value).
            /*CONCEPT OF PROJECTION*/
            IEnumerable<SelectListItem> categories = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
            });
            // now that we retrieved the list, how to pass it?
            // 1. ViewBag: dynamic, temprory per request, good for messages not exisiting in the Model
            // ViewBag.key = value;
            // it is a wrapper around ViewData
            // ViewBag.CategoryList = categories;
            // <select asp-for="CategoryId" asp-items="ViewBag.CategoryList" class="form-select border-0 shadow">
            // 2. ViewData: same as ViewBag but Dictionary type and must be type cast before used
            // ViewData["CategoryList"] = categories;
            // <select asp-for="CategoryId" asp-items="@(ViewData["CategoryList"] as IEnumerable<SelectListItem>)" class="form-select border-0 shadow">
            // 3. TempData: same, needs to be casted before used, error or success message, can be used between requests
            // but disappears after some time, think of it as short life session.
            // 4. ViewModels: Convenient one, static (Model that is specific to a View).
            ProductVM pvm = new() 
            {
                CategoryList = categories,
                Product = new Product(),
            };
            // this means Create
            if (id == 0 || id == null) { return View(pvm); }
            // we have an id so Update
            else
            {
                // populate Product
                pvm.Product = _unitOfWork.Product.Get(p => p.Id == id);
                return View(pvm);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM pvm, IFormFile? file)
        {
            // if (pvm == null) ModelState.AddModelError("Title", "Title cannot be empty!!!");

            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(pvm.Product!);
                TempData["success"] = "Created Product Successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                pvm.CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                });
                return View(pvm);
            }
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound(); 

            Product? productToDelete = _unitOfWork.Product.Get(p => p.Id == id);

            if (productToDelete == null) return NotFound(); 

            _unitOfWork.Product.Delete(productToDelete);
            return RedirectToAction("Index");
        }
    }
}
