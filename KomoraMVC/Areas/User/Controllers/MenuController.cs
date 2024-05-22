using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting.Internal;
using System.Runtime.Remoting;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Komora.Areas.User.Controllers
{
    /// <summary>
    /// Controller that manages the Menu model
    /// </summary>
    [Area("User")]
    [Authorize]
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            var MenuList = _unitOfWork.Menu.GetAll(u => u.UserId == userId);
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
            MenuVM menuVM;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            Menu menuObj = new Menu();
            menuObj.UserId = userId;

            var meals = _unitOfWork.Meal.GetAll(u => u.UserId == userId).ToList();

            var menuRecipes = new List<MenuRecipe>();

            foreach (var meal in meals)
            {
                menuRecipes.Add(new MenuRecipe());
            }

            menuVM = new MenuVM
            {
                MealList = _unitOfWork.Meal.GetAll(u => u.UserId == userId).Select(m => new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString()
                }),
                RecipeMealDict = meals.ToDictionary(
                    meal => meal,  // Key selector
                    meal => _unitOfWork.Recipe.GetAll(r => r.MealId == meal.Id) // Assuming Recipes have a MealId foreign key
                           .Select(r => new SelectListItem
                           {
                               Text = r.Name,
                               Value = r.Id.ToString()
                           })
                ),
                Menu = menuObj,
                MenuRecipes = menuRecipes
            };


            if (id == null || id == 0)
            {
                return View(menuVM);
            }
            else
            {
                menuVM.Menu = _unitOfWork.Menu.Get(u => u.Id == id);
                menuVM.MenuRecipes = _unitOfWork.MenuRecipe.GetAll(pr => pr.MenuId == id).ToList();
                menuVM.Status = menuVM.Menu.Status;
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
            obj.Menu.Status = obj.Status;
            if (obj.Menu.Id == 0)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                obj.Menu.UserId = userId;

                _unitOfWork.Menu.Add(obj.Menu);
                _unitOfWork.Save();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                obj.Menu.UserId = userId;

                _unitOfWork.Menu.Update(obj.Menu);
                _unitOfWork.Save();
            }

            bool hasErrors = false;
            string errorMessage = "";

            if (obj.MenuRecipes != null)
            {
                foreach (var menuRecipe in obj.MenuRecipes)
                {
                    menuRecipe.MenuId = obj.Menu.Id;

                    if (string.IsNullOrWhiteSpace(menuRecipe.RecipeId.ToString()) || menuRecipe.RecipeId == 0)
                    {
                        errorMessage += "Recipe is required. ";
                        hasErrors = true;
                    }

                    // Припускаємо, що UnitId також є обов'язковим
                    

                    // Якщо немає помилок, додаємо або оновлюємо записи
                    if (!hasErrors)
                    {
                        if (menuRecipe.Id == 0)
                        {
                            _unitOfWork.MenuRecipe.Add(menuRecipe);
                        }
                        else
                        {
                            _unitOfWork.MenuRecipe.Update(menuRecipe);
                        }
                    }
                }

                if (hasErrors)
                {
                    TempData["error"] = errorMessage; // Зберігаємо помилки у TempData
                }
                else
                {
                    _unitOfWork.Save();
                    TempData["success"] = "Menu added successfully.";
                    return RedirectToAction("Index"); // Переходимо до списку рецептів, якщо все в порядку
                }
            }
            return RedirectToAction("Index");

            //if (ModelState.IsValid)
            //{
            //    obj.Menu.Status = obj.Status;
            //    if (obj.Menu.Id == 0)
            //    {
            //        _unitOfWork.Menu.Add(obj.Menu);
            //    }
            //    else
            //    {
            //        _unitOfWork.Menu.Update(obj.Menu);
            //    }

            //    _unitOfWork.Save();
            //    if (obj.Menu.Id == 0)
            //    {
            //        TempData["success"] = "Menu added successfully.";

            //    }
            //    else
            //    {
            //        TempData["success"] = "Menu updated successfully.";

            //    }
            //    return RedirectToAction("Index");
            //}
            //else
            //{
            //    obj.MealList = _unitOfWork.Meal.GetAll().Select(i => new SelectListItem
            //    {
            //        Text = i.Name,
            //        Value = i.Id.ToString()
            //    });
            //    obj.RecipeList = _unitOfWork.Recipe.GetAll().Select(i => new SelectListItem
            //    {
            //        Text = i.Name,
            //        Value = i.Id.ToString()
            //    });
            //    return View(obj);

            //}
        }

        [HttpDelete]
        public IActionResult DeleteMenuRecipe(int? id)
        {
            var menuRecipeToBeDeleted = _unitOfWork.MenuRecipe.Get(u => u.Id == id);
            if (menuRecipeToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }



            _unitOfWork.MenuRecipe.Remove(menuRecipeToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<Menu> objMenuList = _unitOfWork.Menu.GetAll(u => u.UserId == userId).ToList();
            return Json(new { data = objMenuList });
        }

        [HttpGet]
        public IActionResult GetAllById()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<Menu> objMenuList = _unitOfWork.Menu.GetAll(u => u.UserId == userId, includeProperties: "Meal,Recipe").ToList();
            return Json(new { data = objMenuList });
        }
    }
}

