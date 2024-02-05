using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (product == null) ModelState.AddModelError("Title", "Title cannot be empty!!!");

            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(product!);
                TempData["success"] = "Created Product Successfully!";
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? productToEdit = _unitOfWork.Product.Get(p => p.Id == id);
            return View(productToEdit);

        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(product);
                // index page updated
                return RedirectToAction("Index");
            }
            // Model error will appear there
            return View();
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) { return NotFound(); }

            Product? productToDelete = _unitOfWork.Product.Get(p => p.Id == id);

            if (productToDelete == null) { return NotFound(); }

            _unitOfWork.Product.Delete(productToDelete);
            return RedirectToAction("Index");
        }
    }
}
