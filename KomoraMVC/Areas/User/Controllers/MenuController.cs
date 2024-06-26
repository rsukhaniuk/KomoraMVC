﻿using Komora.DataAccess.Data;
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
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="unitOfWork"></param>
        public MenuController(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironment, ApplicationDbContext db)
        {
            this._unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
            this._db = db;
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
        /// Method that returns the view with the form to create or update a menu
        /// </summary>
        /// <param name="id">
        /// id of the menu to be updated or created
        /// </param>
        /// <returns>
        /// View with the form to create or update a menu
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
        /// HttpPost method that creates or updates a menu
        /// </summary>
        /// <param name="obj">
        /// menu to be created or updated
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Upsert(MenuVM obj, IFormFile? file)
        {

            obj.Menu.Status = obj.Status;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            obj.Menu.UserId = userId;

  

            var calcOrders = CalculateOrders(obj, 1.0); // tolerance



            if (calcOrders.Count == 0)
            {
                var calcOrderQuan = CalculatePlanQuan(obj, 1.0);
                foreach (var item in calcOrderQuan)
                {
                    var inventoryItems = _unitOfWork.Inventory
                        .GetAll(i => i.ProductId == item.ProductId)
                        .OrderBy(i => i.ExpirationDate == DateTime.MinValue ? 1 : 0)  // Prioritize records without DateTime.MinValue
                        .ThenBy(i => i.ExpirationDate)  // Then sort by date where not MinValue
                        .ToList();

                    var totalPlanToAdd = item.PlanQuan;
                    foreach (var inventoryItem in inventoryItems)
                    {
                        if (totalPlanToAdd <= 0) break;

                        var possibleToAdd = inventoryItem.IncomeQuantity - inventoryItem.PlanQuantity;
                        if (possibleToAdd > 0)
                        {
                            var toAdd = Math.Min(possibleToAdd, totalPlanToAdd);
                            inventoryItem.PlanQuantity = Math.Round(inventoryItem.PlanQuantity + toAdd, 3, MidpointRounding.AwayFromZero);
                            inventoryItem.PlanDate = DateTime.Now;
                            inventoryItem.RemainQuantity = Math.Round(inventoryItem.IncomeQuantity - inventoryItem.PlanQuantity, 3, MidpointRounding.AwayFromZero);
                            inventoryItem.Remaindate = DateTime.Now;
                            totalPlanToAdd -= toAdd;
                        }
                        _unitOfWork.Inventory.Update(inventoryItem);
                    }

                }

                if (obj.Menu.Id == 0)
                {
                    _unitOfWork.Menu.Add(obj.Menu);
                    _unitOfWork.Save();
                }
                else
                {
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
                        return Json(new { success = true, message = "Menu saved successfully. There are enough products" });
                    }
                }
                return RedirectToAction("Index");
            }
            else
            {
                var orders = CalculateOrders(obj, 1.2); // tolerance
                ShoppingListVM shoppingListVM = new ShoppingListVM
                {
                    OrderList = orders,
                    Menu = obj.Menu,
                    MenuRecipes = obj.MenuRecipes,
                    Status = obj.Status
                };
                return Json(new { success = false, message = "Insufficient resources to create the menu.", shoppingListVM = shoppingListVM });
            }
        }

        /// <summary>
        /// Method that calculates the orders for a menu
        /// </summary>
        /// <param name="menuVM">menu ViewModel</param>
        /// <param name="tolerance">tolerance</param>
        /// <returns></returns>
        public List<OrderVM> CalculateOrders(MenuVM menuVM, double tolerance)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var today = DateTime.Today; // Define the date limit for filtering menus.
            var isMenuActive = menuVM.Menu.Status == true; // Check the status of the menu
            var menuDate = menuVM.Menu.Date; // Get the date from the Menu object

            if (menuDate >= today && isMenuActive)
            {

                var recipeIds = menuVM.MenuRecipes.Select(mr => mr.RecipeId).ToList(); // Extract only RecipeIds first

                // Fetch necessary base data without trying to filter on the in-memory list
                var productData = _db.Products
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        CategoryName = p.Category.Name,
                        p.Price,
                        p.Quantity,
                        Unit = p.Unit.Name,
                        ProductRecipes = _db.ProductRecipe
                            .Where(pr => pr.ProductId == p.Id && recipeIds.Contains(pr.RecipeId)) // Only take ProductRecipes that match the RecipeIds
                            .Join(_db.Recipes,
                                pr => pr.RecipeId,
                                r => r.Id,
                                (pr, r) => new { ProductRecipe = pr, Recipe = r }) // Join here is fine as it involves only DB sets
                            .ToList(),
                        Remains = _db.Inventory
                            .Where(i => i.ProductId == p.Id && i.UserId == userId)
                            .Sum(i => i.IncomeQuantity - i.PlanQuantity - i.WasteQuantity)
                    })
                    .ToList(); // Execute the DB query and retrieve the results

                // Now apply any in-memory operations
                var finalData = productData.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.CategoryName,
                    p.Price,
                    p.Quantity,
                    p.Unit,
                    PlanQuantitiesInfo = p.ProductRecipes
                        .SelectMany(pr => menuVM.MenuRecipes
                            .Where(mr => mr.RecipeId == pr.ProductRecipe.RecipeId)
                            .Select(mr => new { mr.Servings, pr.ProductRecipe.Quantity }))
                        .ToList(),
                    p.Remains
                }).ToList();



                // Process the calculations in memory
                var orders = finalData
                    .Select(p => new OrderVM
                    {
                        ProductId = p.Id,   
                        ProductName = p.Name,
                        CategoryName = p.CategoryName,
                        OrderQuan = Math.Round(Math.Ceiling((Math.Max(0, p.PlanQuantitiesInfo.Sum(x => x.Servings * x.Quantity * tolerance)
                            - Math.Max(0, p.Remains))) / p.Quantity) * p.Quantity, 3, MidpointRounding.AwayFromZero),
                        Unit = p.Unit,
                        OrderPrice = (Math.Ceiling((Math.Max(0, p.PlanQuantitiesInfo.Sum(x => x.Servings * x.Quantity * tolerance)
                            - Math.Max(0, p.Remains))) / p.Quantity) * p.Price),
                        PlanQuan = p.PlanQuantitiesInfo.Sum(x => x.Servings * x.Quantity),
                    })
                    .Where(p => p.OrderQuan > 0)
                    .ToList();



                return orders;
            }
            else
            {
                return new List<OrderVM>();
            }
        }

        /// <summary>
        /// Method that calculates the plan quantities for a menu
        /// </summary>
        /// <param name="menuVM">menu ViewModel</param>
        /// <param name="tolerance">tolerance</param>
        /// <returns></returns>
        public List<OrderVM> CalculatePlanQuan(MenuVM menuVM, double tolerance)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var today = DateTime.Today; // Define the date limit for filtering menus.
            var isMenuActive = menuVM.Menu.Status == true; // Check the status of the menu
            var menuDate = menuVM.Menu.Date; // Get the date from the Menu object

            if (menuDate >= today && isMenuActive)
            {

                var recipeIds = menuVM.MenuRecipes.Select(mr => mr.RecipeId).ToList(); // Extract only RecipeIds first

                // Fetch necessary base data without trying to filter on the in-memory list
                var productData = _db.Products
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        CategoryName = p.Category.Name,
                        p.Price,
                        p.Quantity,
                        Unit = p.Unit.Name,
                        ProductRecipes = _db.ProductRecipe
                            .Where(pr => pr.ProductId == p.Id && recipeIds.Contains(pr.RecipeId)) // Only take ProductRecipes that match the RecipeIds
                            .Join(_db.Recipes,
                                pr => pr.RecipeId,
                                r => r.Id,
                                (pr, r) => new { ProductRecipe = pr, Recipe = r }) // Join here is fine as it involves only DB sets
                            .ToList(),
                        Remains = _db.Inventory
                            .Where(i => i.ProductId == p.Id && i.UserId == userId)
                            .Sum(i => i.IncomeQuantity - i.PlanQuantity - i.WasteQuantity)
                    })
                    .ToList(); // Execute the DB query and retrieve the results

                // Now apply any in-memory operations
                var finalData = productData.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.CategoryName,
                    p.Price,
                    p.Quantity,
                    p.Unit,
                    PlanQuantitiesInfo = p.ProductRecipes
                        .SelectMany(pr => menuVM.MenuRecipes
                            .Where(mr => mr.RecipeId == pr.ProductRecipe.RecipeId)
                            .Select(mr => new { mr.Servings, pr.ProductRecipe.Quantity }))
                        .ToList(),
                    p.Remains
                }).ToList();



                // Process the calculations in memory
                var orders = finalData
                    .Select(p => new OrderVM
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        PlanQuan = p.PlanQuantitiesInfo.Sum(x => x.Servings * x.Quantity),
                    })
                    .ToList();



                return orders;
            }
            else
            {
                return new List<OrderVM>();
            }
        }



        /// <summary>
        /// HttpDelete method that deletes a menu recipe
        /// </summary>
        /// <param name="id">if of menurecipe</param>
        /// <returns></returns>
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
        /// HttpDelete method that deletes a menu
        /// </summary>
        /// <param name="id">id of the menu to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var menuToBeDeleted = _unitOfWork.Menu.Get(u => u.Id == id);
            var menuRecipesToBeDeleted = _unitOfWork.MenuRecipe.GetAll(u => u.MenuId == id).ToList();

            MenuVM menuVMToBeDeleted = new MenuVM
            {
                Menu = menuToBeDeleted,
                MenuRecipes = menuRecipesToBeDeleted
            };

            var calcOrderQuan = CalculatePlanQuan(menuVMToBeDeleted, 1.0);
            foreach (var item in calcOrderQuan)
            {
                var inventoryItems = _unitOfWork.Inventory
                    .GetAll(i => i.ProductId == item.ProductId)
                    .OrderBy(i => i.ExpirationDate == DateTime.MinValue ? 1 : 0)  // Prioritize records without DateTime.MinValue
                    .ThenBy(i => i.ExpirationDate)  // Then sort by date where not MinValue
                    .ToList();

                var totalPlanToAdd = item.PlanQuan;
                foreach (var inventoryItem in inventoryItems)
                {
                    if (totalPlanToAdd <= 0) break;

                    var possibleToDelete =  inventoryItem.PlanQuantity;
                    {
                        var toAdd = Math.Min(possibleToDelete, totalPlanToAdd);
                        inventoryItem.PlanQuantity = Math.Round(inventoryItem.PlanQuantity - toAdd, 3, MidpointRounding.AwayFromZero);
                        inventoryItem.PlanDate = DateTime.Now;
                        inventoryItem.RemainQuantity = Math.Round(inventoryItem.IncomeQuantity - inventoryItem.PlanQuantity, 3, MidpointRounding.AwayFromZero);
                        inventoryItem.Remaindate = DateTime.Now;
                        totalPlanToAdd -= toAdd;
                    }
                    _unitOfWork.Inventory.Update(inventoryItem);
                }

            }

            if (menuToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            

            _unitOfWork.Menu.Remove(menuToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        /// <summary>
        /// HttpGet method that gets all menus for a user in JSON format
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<Menu> objMenuList = _unitOfWork.Menu.GetAll(u => u.UserId == userId).ToList();
            return Json(new { data = objMenuList });
        }

        
    }
}

