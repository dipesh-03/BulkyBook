using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class CompanyController : Controller
    {
        #region Properties
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        #region Constructors
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Actions
        public IActionResult Index()
        {
            return View();
        }


        //GET
        public IActionResult Upsert(int? id)
        {
            Company company = new Company();

            if (id == null || id == 0)
            {
                return View(company);
            }
            else
            {
                //update
                company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
                return View(company);
            }

        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {

            if (ModelState.IsValid)
            {
                if (obj.Id != 0)
                {
                    _unitOfWork.Company.Update(obj);
                    TempData["success"] = "Company updated successfully";

                }
                else
                {
                    _unitOfWork.Company.Add(obj);
                    TempData["success"] = "Company added successfully";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }

            return View(obj);
        }
        #endregion

        #region API CALLS

        /// <summary>
        /// Endpoint : /Admin/Compaby/GetAll
        /// Request Type : GET
        /// Return Type : Json
        /// </summary>
        /// <returns>List of companies stored in the company table</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }

        /// <summary>
        /// Endpoint : /Admin/Company/Delete/{id?}
        /// Request Type : Delete
        /// Function : Delete company with given id
        /// </summary>
        /// <returns>Success/Fail</returns>
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Company.GetFirstOrDefault(u=>u.Id == id);
            if(obj == null)
            {
                return Json(new {success = false, message = "Error while Deleting"});
            }

            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Company deleted successfully" });
        }
        #endregion
    }
}
