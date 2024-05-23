using Komora.DataAccess.Data;
using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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

        public List<CalculateMenuVM> PlanMenus(DateTime startDate, DateTime endDate, string userId, double budget, int servingsPerMeal)
        {
            var plannedMenus = new List<CalculateMenuVM>();
            double totalCost = 0;

            // Pre-load necessary data
            var meals = _db.Meals.Where(m => m.UserId == userId).ToList();
            var recipes = _db.Recipes.Where(r => meals.Select(m => m.Id).Contains(r.MealId) && r.UserId == userId).ToList();
            var productRecipes = _db.ProductRecipe.Where(pr => recipes.Select(r => r.Id).Contains(pr.RecipeId)).ToList();
            var products = _db.Products.ToList();
            var productPriceMap = products.ToDictionary(p => p.Id, p => p.Price);

            // Fetch and clone inventory to operate on a virtual level
            var virtualInventory = _db.Inventory
                .Where(i => i.UserId == userId)
                .ToList()
                .ToDictionary(i => i.ProductId, i => new InventoryItem
                {
                    ProductId = i.ProductId,
                    RemainQuantity = i.RemainQuantity
                });

            // Loop through each planning day
            for (DateTime day = startDate; day <= endDate; day = day.AddDays(1))
            {
                var dailyMenu = new Menu { Date = day, UserId = userId, Status = true };
                var dailyMenuRecipes = new List<MenuRecipe>();
                bool canPrepareAll = true;

                foreach (var meal in meals)
                {
                    var mealRecipes = recipes.Where(r => r.MealId == meal.Id).ToList();
                    bool canPrepare = true;
                    double dailyRecipeCost = 0;

                    Recipe newRecipe = new Recipe();
                    var newIngredients = new List<ProductRecipe>();
                    foreach (var recipe in mealRecipes)
                    {

                        canPrepare = true;
                        dailyRecipeCost = 0;
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
                                // Calculate cost using pre-loaded price map
                                dailyRecipeCost += requiredQuantity * productPriceMap[ingredient.ProductId];
                                //inventoryItem.RemainQuantity -= requiredQuantity;  // Deduct from virtual inventory

                            }
                        }
                        if (canPrepare)
                        {
                            newRecipe = recipe;
                            newIngredients = productRecipes.Where(pr => pr.RecipeId == newRecipe.Id).ToList();
                        }
                        
                    }
                    if (newIngredients.Count > 0)
                    {
                        dailyMenuRecipes.Add(new MenuRecipe { MenuId = dailyMenu.Id, RecipeId = newRecipe.Id, Servings = servingsPerMeal });
                        totalCost += dailyRecipeCost;
                        foreach (var ingredient in newIngredients)
                        {
                            var requiredQuantity = ingredient.Quantity * servingsPerMeal;
                            virtualInventory[ingredient.ProductId].RemainQuantity -= requiredQuantity;  // Add back to virtual inventory
                        }
                    }
                }

                if (dailyMenuRecipes.Count > 0)
                {
                    plannedMenus.Add(new CalculateMenuVM { Menu = dailyMenu, MenuRecipes = dailyMenuRecipes, TotalCost = totalCost, CanPrepare = canPrepareAll });
                }

                  // Stop if budget exceeded
            }

            return plannedMenus;
        }



        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var menus = PlanMenus(DateTime.Now, DateTime.Now.AddDays(2), userId, 4000, 3);



            return RedirectToAction("Index", "Home");
        }

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
