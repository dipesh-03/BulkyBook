using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CoverController : Controller
    {
        private readonly IUnitOfWork _unitOfWork ;
        public CoverController(IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork ;
        }

        //GET
        public ActionResult Index()
        {
            IEnumerable<CoverType> ls = _unitOfWork.CoverType.GetAll();
            return View(ls);
        }

        //get
        public ActionResult Create() {
            return View();
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Add(coverType);
                _unitOfWork.Save();

                TempData["success"] = "Cover Type added successfully.";

                return RedirectToAction("Index");
            }

            return View(coverType);
        }

        //get
        public IActionResult Edit(int ? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _unitOfWork.CoverType.GetFirstOrDefault(e => e.Id == id);

            if(obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(obj);
                _unitOfWork.Save();

                TempData["success"] = "Cover Type updated successfully.";

                return RedirectToAction("Index");
            }
            return View(obj);
        }


        //get
        public ActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _unitOfWork.CoverType.GetFirstOrDefault(e => e.Id == id);

            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(CoverType obj)
        {

            if(obj == null)
            {
                return NotFound();
            }

            _unitOfWork.CoverType.Remove(obj);
            _unitOfWork.Save();

            TempData["success"] = "Cover Type deleted successfully.";

            return RedirectToAction("Index");
        }
    }
}
