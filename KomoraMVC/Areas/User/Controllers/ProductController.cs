using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Models.ViewModels;
using Komora.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Komora.Areas.User.Controllers
{

    /// <summary>
    /// Controller that manages the Product model
    /// </summary>
    [Area("User")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="hostingEnvironment"></param>
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Method that returns the view with the list of products
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_User)]
        public IActionResult Index()
        {
            var ProductList = _unitOfWork.Product.GetAll(includeProperties: "Category,Unit");
            return View(ProductList);
        }

        /// <summary>
        /// Method that returns the view with the form to create or update a product
        /// </summary>
        /// <param name="id">
        /// id of the product to be updated or created
        /// </param>
        /// <returns>
        /// View with the form to create or update a product
        /// </returns>
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            IEnumerable<SelectListItem> UnitList = _unitOfWork.Unit.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            ProductVM productVM = new ProductVM()
            {
                CategoryList = CategoryList,
                UnitList = UnitList,
                Product = new Product()

            };

            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }

        /// <summary>
        /// HttpPost method that creates or updates a product
        /// </summary>
        /// <param name="obj">
        /// product to be created or updated
        /// </param>
        /// <param name="file">
        /// Image of product
        /// </param>
        /// <returns></returns>
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostingEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath + @"\images\product", fileName);

                    if (!string.IsNullOrEmpty(obj.Product.imgUrl))
                    {
                        string oldFilePath = Path.Combine(wwwRootPath + obj.Product.imgUrl);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    using (var fileStream = new FileStream(productPath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.Product.imgUrl = @"\images\product\" + fileName;
                }

                if (obj.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.Product);
                    TempData["success"] = "Product added successfully.";

                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);
                    TempData["success"] = "Product updated successfully.";

                }

                _unitOfWork.Save();
                return RedirectToAction("Index", new { Area = "User" });
            }
            else
            {
                obj.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                obj.UnitList = _unitOfWork.Unit.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                return View(obj);

            }
        }

        /// <summary>
        /// HttpDelete method that deletes a product
        /// </summary>
        /// <param name="id">id of the product to be deleted</param>
        /// <returns></returns>
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin)]
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath + productToBeDeleted.imgUrl);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        /// <summary>
        /// Method that returns the list of products in json format
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_User)]
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category,Unit").ToList();
            return Json(new { data = objProductList });
        }

        
    }
}

