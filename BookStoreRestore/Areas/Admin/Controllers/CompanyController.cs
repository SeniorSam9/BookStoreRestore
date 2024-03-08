using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStoreRestore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Admin_Role)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> Companies = _unitOfWork.Company.GetAll().ToList();
            
            return View(Companies);
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {

            if (id == 0 || id == null) { return View(new Company()); }
            // we have an id so Update
            else
            {
                // populate Company
                Company companyFromDb = _unitOfWork.Company.Get(p => p.Id == id);
                return View(companyFromDb);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
     

            if (ModelState.IsValid)
            {
               
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company!);
                    TempData["success"] = "Created Company Successfully!";
                }
                else
                {
                    _unitOfWork.Company.Update(company!);
                    TempData["success"] = "Updated Company Successfully!";
                }
                
                return RedirectToAction("Index");
            }
            else
            {
                return View(company);
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companies = _unitOfWork.Company.GetAll().ToList();
            return Json(new { success = true, data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Company? companyToBeDeleted = _unitOfWork.Company.Get(p => p.Id == id);

            if (companyToBeDeleted == null)
            {
                return Json(new
                {
                    success = false,
                    data = @"Failed to delete,
                            no such product exist! 
                            Make sure to delete a valid existing id."
                });
            }

            _unitOfWork.Company.Delete(companyToBeDeleted);
            return Json(new { success = true, data = "Deleted successfully!" });
        }
        #endregion
    }
}
