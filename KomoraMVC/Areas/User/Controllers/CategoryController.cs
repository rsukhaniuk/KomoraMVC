using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Komora.Areas.User.Controllers
{
    /// <summary>
    /// Controller that manages the Category model
    /// </summary>
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        public CategoryController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Method that returns the view with the list of categories
        /// </summary>
        /// <returns></returns>
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
                return RedirectToAction("Index");
            }
            else
            {
                return View(obj);
            }
        }

        /// <summary>
        /// Method that deletes a category
        /// </summary>
        /// <param name="id">id of the category to be deleted</param>
        /// <returns></returns>
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = _unitOfWork.Category.Get(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        /// <summary>
        /// HttpPost method that deletes a category
        /// </summary>
        /// <param name="id">id of the category to be deleted</param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            var categotyToBeDeleted = _unitOfWork.Category.Get(c => c.Id == id);

            if (categotyToBeDeleted == null)
            {
                return NotFound();
            }
            _unitOfWork.Category.Remove(categotyToBeDeleted);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted  successfully.";
            return RedirectToAction("Index");
        }
    }
}
