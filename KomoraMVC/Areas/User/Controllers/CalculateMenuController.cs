using Komora.DataAccess.Data;
using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Komora.Areas.User.Controllers
{
    /// <summary>
    /// Controller that manages the Category model
    /// </summary>
    [Area("User")]
    [Authorize]
    public class CalculateMenuController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        public CalculateMenuController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            this._unitOfWork = unitOfWork;
            this._db = db;
        }

        public List<CalculateMenuVM> PlanMenus(DateTime startDate, DateTime endDate, string userId, int servingsPerMeal, int TotalCalories, bool IsVeg, bool IsRepeat )
        {
            var plannedMenus = new List<CalculateMenuVM>();
            double totalCost = 0;
            int totalDays = (endDate - startDate).Days + 1;
            int dailyCalorieLimit = TotalCalories / totalDays;  // Daily calorie limit per person


            // Pre-load necessary data
            var meals = _db.Meals.Where(m => m.UserId == userId).ToList();
            var recipes = _db.Recipes.Where(r => meals.Select(m => m.Id).Contains(r.MealId) && r.UserId == userId).ToList();
            var productRecipes = _db.ProductRecipe.Where(pr => recipes.Select(r => r.Id).Contains(pr.RecipeId)).ToList();
            var products = _db.Products.ToList();
            var productPriceMap = products.ToDictionary(p => p.Id, p => p.Price);

            // Fetch and clone inventory to operate on a virtual level
            var virtualInventory = _db.Inventory
                .Where(i => i.UserId == userId)
                .GroupBy(i => i.ProductId)  // Group inventory items by ProductId
                .Select(g => new {
                    ProductId = g.Key,
                    TotalRemainQuantity = g.Sum(item => item.RemainQuantity)  // Sum up all quantities for each ProductId
                })
                .ToDictionary(
                    item => item.ProductId,
                    item => new InventoryItem
                    {
                        ProductId = item.ProductId,
                        RemainQuantity = item.TotalRemainQuantity  // Use the summed quantity
                    }
                );

            // Loop through each planning day
            for (DateTime day = startDate; day <= endDate; day = day.AddDays(1))
            {
                var dailyMenu = new Menu { Date = day, UserId = userId, Status = true };
                var dailyMenuRecipes = new List<MenuRecipe>();
                bool canPrepareAll = true;
                int dailyCaloriesConsumed = 0;

                foreach (var meal in meals)
                {
                    var mealRecipes = recipes.Where(r => r.MealId == meal.Id).ToList();
                    bool canPrepare = true;
                    Dictionary<Recipe, List<ProductRecipe>> suitableRecipes = new Dictionary<Recipe, List<ProductRecipe>>();

                    //Recipe newRecipe = new Recipe();
                    //var newIngredients = new List<ProductRecipe>();
                    foreach (var recipe in mealRecipes)
                    {
                        bool recipeIsVegan = recipe.IsVegetarian;
                        canPrepare = true;
                        double dailyRecipeCost = 0;
                        int recipeCalories = (int)recipe.Calories;
                        var ingredients = productRecipes.Where(pr => pr.RecipeId == recipe.Id).ToList();

                        foreach (var ingredient in ingredients)
                        {
                            var requiredQuantity = ingredient.Quantity * servingsPerMeal;
                            if (!virtualInventory.TryGetValue(ingredient.ProductId, out var inventoryItem) || inventoryItem.RemainQuantity < requiredQuantity)
                            {
                                canPrepare = false;
                                canPrepareAll = false;
                                break;
                            }
                            else
                            {
                                dailyRecipeCost += requiredQuantity * productPriceMap[ingredient.ProductId];
                            }
                        }
                        if (canPrepare && dailyCaloriesConsumed + recipeCalories <= dailyCalorieLimit && ((IsVeg && recipeIsVegan) || !IsVeg))
                        {
                            suitableRecipes.Add(recipe, productRecipes.Where(pr => pr.RecipeId == recipe.Id).ToList());
                        }
                        
                    }
                    if (suitableRecipes.Count > 0)
                    {

                        if (!IsRepeat)
                        {
                            // Find the menu from the previous day
                            var previousMenu = plannedMenus.FirstOrDefault(menu => menu.Menu.Date == day.AddDays(-1));
                            HashSet<int> previousDayRecipeIds = new HashSet<int>();
                            if (previousMenu != null)
                            {
                                foreach (var recipePair in suitableRecipes)
                                {
                                    var currRecipe = recipePair.Key;
                                    var currIngredients = recipePair.Value;
                                    if (!previousMenu.MenuRecipes.Any(mr => mr.RecipeId == currRecipe.Id) /*|| suitableRecipes.Count == 1*/)
                                    {
                                        dailyMenuRecipes.Add(new MenuRecipe { MenuId = dailyMenu.Id, RecipeId = currRecipe.Id, Servings = servingsPerMeal });
                                        dailyCaloriesConsumed += (int)currRecipe.Calories;
                                        // Calculate the required quantity of each ingredient and update virtual inventory
                                        foreach (var ingredient in currIngredients)
                                        {
                                            var requiredQuantity = ingredient.Quantity * servingsPerMeal;
                                            virtualInventory[ingredient.ProductId].RemainQuantity -= requiredQuantity;  // Deduct from virtual inventory
                                        }
                                        break;
                                    }
                                }

                            }
                            else
                            {
                                var recipe = suitableRecipes.First().Key;
                                var ingredients = suitableRecipes.First().Value;

                                dailyMenuRecipes.Add(new MenuRecipe { MenuId = dailyMenu.Id, RecipeId = recipe.Id, Servings = servingsPerMeal });
                                dailyCaloriesConsumed += (int)recipe.Calories;
                                // Calculate the required quantity of each ingredient and update virtual inventory
                                foreach (var ingredient in ingredients)
                                {
                                    var requiredQuantity = ingredient.Quantity * servingsPerMeal;
                                    virtualInventory[ingredient.ProductId].RemainQuantity -= requiredQuantity;  // Deduct from virtual inventory
                                }
                            }
                        }
                        else
                        {

                            var recipeWithMinCalories = suitableRecipes
                                .OrderBy(r => r.Key.Calories)  // Order by calories ascending
                                .FirstOrDefault();

                            var recipe = recipeWithMinCalories.Key;
                            var ingredients = recipeWithMinCalories.Value;

                            dailyMenuRecipes.Add(new MenuRecipe { MenuId = dailyMenu.Id, RecipeId = recipe.Id, Servings = servingsPerMeal });
                            dailyCaloriesConsumed += (int)recipe.Calories;
                            // Calculate the required quantity of each ingredient and update virtual inventory
                            foreach (var ingredient in ingredients)
                            {
                                var requiredQuantity = ingredient.Quantity * servingsPerMeal;
                                virtualInventory[ingredient.ProductId].RemainQuantity -= requiredQuantity;  // Deduct from virtual inventory
                            }
                        }
                        
                    }
                }

                if (dailyMenuRecipes.Count > 0)
                {
                    plannedMenus.Add(new CalculateMenuVM { Menu = dailyMenu, MenuRecipes = dailyMenuRecipes, TotalCost = totalCost, CanPrepare = canPrepareAll, TotalCaloriesMenu = dailyCaloriesConsumed });
                }

                  // Stop if budget exceeded
            }

            return plannedMenus;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Initialize with default values to ensure the form is clear on first access or provide reasonable defaults.
            var viewModel = new MenuPlanningViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(2),
                ServingsPerMeal = 1,
                CalculatedMenus = new List<CalculateMenuVM>(), // Initially empty
                TotalCalories = 0,
                IsVegan = false,
                IsRepeat = true,
        };

            // Fetch the recipes
            var recipes = _db.Recipes.ToList();  // Ensure the query is executed with ToList if necessary

            // Build the dictionary manually
            foreach (var recipe in recipes)
            {
                if (!viewModel.RecipeNames.ContainsKey(recipe.Id))  // Check to avoid key conflicts
                {
                    viewModel.RecipeNames.Add(recipe.Id, (_unitOfWork.Meal.Get(u => u.Id == recipe.MealId).Name, recipe.Name ));
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Index(MenuPlanningViewModel model)
        {
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                model.CalculatedMenus = PlanMenus(model.StartDate, model.EndDate, userId, model.ServingsPerMeal, (int)model.TotalCalories, model.IsVegan, model.IsRepeat);
                //model.RecipeNames = _db.Recipes
                //               .Select(r => new { r.Id, r.Name })  // Select only necessary fields
                //               .ToDictionary(r => r.Id, r => r.Name);  // Convert to dictionary
                var recipes = _db.Recipes.ToList();
                foreach (var recipe in recipes)
                {
                    if (!model.RecipeNames.ContainsKey(recipe.Id))  // Check to avoid key conflicts
                    {
                        model.RecipeNames.Add(recipe.Id, (_unitOfWork.Meal.Get(u => u.Id == recipe.MealId).Name, recipe.Name ));
                    }
                }

                return View(model); // Return to the Index view with the calculated data
            }

            return View(model); // Return the model to display any validation errors
        }

        [HttpPost]
        public IActionResult Insert (MenuPlanningViewModel model)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Fetch all meal types once at the beginning
            var allMeals = _unitOfWork.Meal.GetAll(m => m.UserId == userId).ToList();
            var allMealIds = new HashSet<int>(allMeals.Select(m => m.Id)); // Store all meal IDs for quick lookup

            // Prepare a map of RecipeIds to MealIds
            var recipeToMealMap = _unitOfWork.Recipe.GetAll()
                                    .Where(r => r.MealId != null)
                                    .ToDictionary(r => r.Id, r => r.MealId);

            StringBuilder errorMessages = new StringBuilder();

            foreach (var menuVM in model.CalculatedMenus)
            {
                // Use the map to get MealIds from RecipeIds in MenuRecipes
                var providedMealIds = new HashSet<int>(
                    menuVM.MenuRecipes
                    .Where(mr => recipeToMealMap.ContainsKey(mr.RecipeId))
                    .Select(mr => recipeToMealMap[mr.RecipeId])
                );

                var missingMealIds = allMealIds.Except(providedMealIds).ToList(); // Get missing meal IDs

                if (missingMealIds.Any())
                {
                    var missingMealNames = _unitOfWork.Meal.GetAll(m => missingMealIds.Contains(m.Id))
                                               .Select(m => m.Name)
                                               .ToList(); // Fetch the names of missing meals
                    string menuDate = menuVM.Menu.Date.HasValue ? menuVM.Menu.Date.Value.ToString("yyyy-MM-dd") : "Date not set";
                    errorMessages.AppendLine($"Menu for {menuDate} is missing recipes for: {String.Join(", ", missingMealNames)}.");
                    errorMessages.Append("\n");
                }
            }

            if (errorMessages.Length > 0)
            {
                return Json(new { success = false, message = errorMessages.ToString() });
            }



            foreach (var menuVM in model.CalculatedMenus)
            {

                menuVM.Menu.Status = true;
                menuVM.Menu.UserId = userId;


                var calcOrderQuan = CalculatePlanQuan(menuVM, 1.0);
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
                            inventoryItem.PlanQuantity = Math.Round(inventoryItem.PlanQuantity + toAdd, 3, MidpointRounding.AwayFromZero); ;
                            inventoryItem.PlanDate = DateTime.Now;
                            inventoryItem.RemainQuantity = Math.Round(inventoryItem.IncomeQuantity - inventoryItem.PlanQuantity, 3, MidpointRounding.AwayFromZero);
                            inventoryItem.Remaindate = DateTime.Now;
                            totalPlanToAdd -= toAdd;
                        }
                        _unitOfWork.Inventory.Update(inventoryItem);
                    }

                }

                if (menuVM.Menu.Id == 0)
                {
                    _unitOfWork.Menu.Add(menuVM.Menu);
                    _unitOfWork.Save();
                }
                else
                {
                    _unitOfWork.Menu.Update(menuVM.Menu);
                    _unitOfWork.Save();
                }

                bool hasErrors = false;
                string errorMessage = "";

                if (menuVM.MenuRecipes != null)
                {
                    foreach (var menuRecipe in menuVM.MenuRecipes)
                    {
                        menuRecipe.MenuId = menuVM.Menu.Id;

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
                        
                    }
                }
                

            }
            TempData["success"] = "Menu added successfully.";

            return Json(new { success = true, message = "Menu added successfully." });
        }


        public List<OrderVM> CalculatePlanQuan(CalculateMenuVM menuVM, double tolerance)
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

        //public IActionResult Index()
        //{
        //    var claimsIdentity = (ClaimsIdentity)User.Identity;
        //    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        //    var menus = PlanMenus(DateTime.Now, DateTime.Now.AddDays(2), userId, 3);



        //    return RedirectToAction("Index", "Home");
        //}

        //[HttpPost]
        //public IActionResult Insert()
        //{
        //    var claimsIdentity = (ClaimsIdentity)User.Identity;
        //    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

        //    var orders = CalculateOrders();
        //    foreach (var item in orders)
        //    {
        //        InventoryItem inventoryItem = new InventoryItem();
        //        inventoryItem.UserId = userId;
        //        inventoryItem.ProductId = item.ProductId;
        //        inventoryItem.RemainQuantity = item.OrderQuan;
        //        inventoryItem.IncomeDate = DateTime.Now;
        //        _unitOfWork.Inventory.Add(inventoryItem);
        //    }

        //    _unitOfWork.Save();
        //    TempData["success"] = "Products added successfully.";
        //    return RedirectToAction("Index");
        //}

    }
}
