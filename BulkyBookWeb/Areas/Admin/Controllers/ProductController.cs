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
    public class ProductController : Controller
    {
        #region Properties
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        #endregion

        #region Constructors
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
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
            //using ViewModels to tighly make VIEW tighly 
            ProductVM productVM = new()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(
                    e => new SelectListItem()
                    {
                        Text = e.Name,
                        Value = e.Id.ToString()
                    }
                ),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(
                e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id.ToString()
                }
                )
            };

            //SelectListItem is used to bind list (text, value) with DropDown menu items
            //if id is null or not provided then create product
            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
            }

        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                //_unitOfWork.CoverType.Update(obj);
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString(); //new generated name of the file
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName); // extension of the file

                    //if image for book is already stored then we need to delete it first
                    if (obj.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }

                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;

                }

                if (obj.Product.Id != 0)
                {
                    _unitOfWork.Product.Update(obj.Product);
                    _unitOfWork.Save();

                    TempData["success"] = "Product updated successfully";

                }
                else
                {
                    _unitOfWork.Product.Add(obj.Product);
                    _unitOfWork.Save();

                    TempData["success"] = "Product created successfully";
                }

                return RedirectToAction("Index");
            }

            return View(obj);
        }
        #endregion

        #region API CALLS

        /// <summary>
        /// Endpoint : /Admin/Product/GetAll
        /// Request Type : GET
        /// </summary>
        /// <returns>returns all data stored in the table in form of JSON</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return Json(new { data = productList });
        }  

        /// <summary>
        /// Endpoint : /Admin/Product/Delete/{id?}
        /// Request Type : Delete
        /// Function : Delete data with given id from the table
        /// </summary>
        /// <returns>Success/Fail</returns>
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Product.GetFirstOrDefault(u=>u.Id == id);
            if(obj == null)
            {
                return Json(new {success = false, message = "Error while Deleting"});
            }

            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Product deleted successfully" });
        }
        #endregion
    }
}
