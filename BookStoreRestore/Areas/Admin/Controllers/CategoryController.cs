using BookStore.DataAccess.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using BookStore.DataAccess.Repository.IRepository;


namespace BookStoreRestore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            // not yet implemented
            List<Category> categoryList = _unitOfWork.Category.GetAll().ToList();
            // pass it to the view
            return View(categoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            // custom validation
            if (category.Name == category.DisplayOrder.ToString())
            {
                // key, value
                // key is attribute of the category class
                // value is the displayed message
                ModelState.AddModelError("Name", "Category name and Display order are matching!!!");
            }
            if (category.Name != null && category.Name.ToLower().Equals("test"))
            {
                // kind of custom error (not related to any attribute)
                // asp validation needs to be set as "All" to be rendered
                ModelState.AddModelError("", "test is not valid name!");
            }
            // ModelState.IsValid will check the validations that we inserted in category model
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                TempData["success"] = "Category Created Successfully!";
                return RedirectToAction("Index");
            }
            /// else keep us in the same view with error message displayed
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // one way to get the category from the DB is by primary key
            Category? categoryFromDb = _unitOfWork.Category.Get(c => c.Id == id);
            // firstordefault if to get data from any attribute not necessarily the primary key
            //Category? categoryFromDb2 = _dbContext.Categories.FirstOrDefault(c => c.Name == Name);
            // if (categoryFromDb == null)
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {

            if (ModelState.IsValid)
            {
                // we dont need to look for the id manually
                // it automatically finds category id and updates it
                _unitOfWork.Category.Update(category);
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category? categoryFromDb = _unitOfWork.Category.Get(c => c.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Delete(categoryFromDb);
            return RedirectToAction("Index");
        }
    }
}
