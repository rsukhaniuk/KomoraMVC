using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting.Internal;
using System.Runtime.Remoting;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Komora.Areas.User.Controllers
{
    /// <summary>
    /// Controller that manages the Menu model
    /// </summary>
    public class MenuController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        public MenuController(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironment)
        {
            this._unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Method that returns the view with the list of menu
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var MenuList = _unitOfWork.Menu.GetAll(includeProperties: "Meal,Recipe");
            return View(MenuList);
        }

        /// <summary>
        /// Method that returns the view with the form to create or update a product
        /// </summary>
        /// <param name="id">
        /// id of the product to be updated
        /// </param>
        /// <returns>
        /// View with the form to create or update a product
        /// </returns>
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> MealList = _unitOfWork.Meal.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            IEnumerable<SelectListItem> RecipeList = _unitOfWork.Recipe.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            MenuVM menuVM = new MenuVM()
            {
                MealList = MealList,
                RecipeList = RecipeList,
                Menu = new Menu()

            };

            if (id == null || id == 0)
            {
                return View(menuVM);
            }
            else
            {
                menuVM.Menu = _unitOfWork.Menu.Get(u => u.Id == id);
                return View(menuVM);
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
        [HttpPost]
        public IActionResult Upsert(MenuVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                obj.Menu.Status = obj.Status;
                if (obj.Menu.Id == 0)
                {
                    _unitOfWork.Menu.Add(obj.Menu);
                }
                else
                {
                    _unitOfWork.Menu.Update(obj.Menu);
                }

                _unitOfWork.Save();
                if (obj.Menu.Id == 0)
                {
                    TempData["success"] = "Menu added successfully.";

                }
                else
                {
                    TempData["success"] = "Menu updated successfully.";

                }
                return RedirectToAction("Index");
            }
            else
            {
                obj.MealList = _unitOfWork.Meal.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                obj.RecipeList = _unitOfWork.Recipe.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                return View(obj);

            }
        }

        /// <summary>
        /// HttpDelete method that deletes a category
        /// </summary>
        /// <param name="id">id of the category to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var menuToBeDeleted = _unitOfWork.Menu.Get(u => u.Id == id);
            if (menuToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            

            _unitOfWork.Menu.Remove(menuToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Menu> objMenuList = _unitOfWork.Menu.GetAll(includeProperties: "Meal,Recipe").ToList();
            return Json(new { data = objMenuList });
        }

        [HttpGet]
        public IActionResult GetAllById()
        {
            List<Menu> objMenuList = _unitOfWork.Menu.GetAll(includeProperties: "Meal,Recipe").ToList();
            return Json(new { data = objMenuList });
        }
    }
}

