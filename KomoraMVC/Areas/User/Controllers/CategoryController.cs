using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Komora.Areas.User.Controllers
{
    /// <summary>
    /// Controller that manages the Category model
    /// </summary>
    [Area("User")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Method that returns the view with the list of categories
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_User)]
        public IActionResult Index()
        {
            var CategoryList = _unitOfWork.Category.GetAll();
            return View(CategoryList);
        }

        /// <summary>
        /// Method that returns the view with the form to create or update a category
        /// </summary>
        /// <param name="id">
        /// id of the category to be updated
        /// </param>
        /// <returns>
        /// View with the form to create or update a category
        /// </returns>
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Category());
            }
            else
            {
                Category categoryObj = _unitOfWork.Category.Get(u => u.Id == id);
                return View(categoryObj);
            }
        }

        /// <summary>
        /// HttpPost method that creates or updates a category
        /// </summary>
        /// <param name="obj">
        /// Category to be created or updated
        /// </param>
        /// <param name="file">
        /// Image of category
        /// </param>
        /// <returns></returns>
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        public IActionResult Upsert(Category obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.Category.Add(obj);
                }
                else
                {
                    _unitOfWork.Category.Update(obj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index", new { Area = "User" });
            }
            else
            {
                return View(obj);
            }
        }

        

        /// <summary>
        /// HttpDelete method that deletes a category
        /// </summary>
        /// <param name="id">id of the category to be deleted</param>
        /// <returns></returns>
        [Area("Admin")]
        [Authorize(Roles = SD.Role_Admin)]
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var categotyToBeDeleted = _unitOfWork.Category.Get(c => c.Id == id);

            if (categotyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Category.Remove(categotyToBeDeleted);
            _unitOfWork.Save();


            return Json(new { success = true, message = "Delete Successful" });
            //return RedirectToAction("Index");
        }
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_User)]
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return Json(new { data = objCategoryList });
        }
    }
}
