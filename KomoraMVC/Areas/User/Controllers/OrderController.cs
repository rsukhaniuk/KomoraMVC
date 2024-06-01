using Komora.DataAccess.Data;
using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Web;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static NuGet.Packaging.PackagingConstants;

namespace Komora.Areas.User.Controllers
{
    /// <summary>
    /// Controller that manages orders
    /// </summary>
    [Area("User")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public List<OrderVM> calculatedOrder { get; set; }

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork">unitOfWork</param>
        /// <param name="db">dbContext</param>
        public OrderController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            this._unitOfWork = unitOfWork;
            this._db = db;
        }

        /// <summary>
        /// Method that returns the view with the list of orders
        /// </summary>
        /// <param name="shoppingListVM"></param>
        /// <returns></returns>
        public IActionResult Index(string shoppingListVM)
        {
            ShoppingListVM ShoppingListVM = JsonConvert.DeserializeObject<ShoppingListVM>(HttpUtility.UrlDecode(shoppingListVM));
            
            return View(ShoppingListVM);
        }

        /// <summary>
        /// HttpPost method that inserts a new order into the database
        /// </summary>
        /// <param name="shoppingListVM">shopping list view model</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Insert(ShoppingListVM shoppingListVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingListVM.Menu.Status = shoppingListVM.Status;

            var menuVM = new MenuVM();
            menuVM.Menu = shoppingListVM.Menu;
            menuVM.MenuRecipes = shoppingListVM.MenuRecipes;
            menuVM.Menu.Status = menuVM.StatusForDisplay;

            var calcOrderQuan = CalculatePlanQuan(menuVM, 1.0);

            foreach (var item in shoppingListVM.OrderList)
            {
                InventoryItem inventoryItem = new InventoryItem();
                inventoryItem.UserId = userId;
                inventoryItem.ProductId = item.ProductId;
                inventoryItem.IncomeQuantity += item.OrderQuan;
                inventoryItem.IncomeDate = DateTime.Now;
                inventoryItem.PlanQuantity = 0;
                inventoryItem.PlanDate = DateTime.Now;
                inventoryItem.RemainQuantity = item.OrderQuan;
                _unitOfWork.Inventory.Add(inventoryItem);
            }
            _unitOfWork.Save();

            

            
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



                //// Обробка випадків, коли не всю кількість можна розподілити
                //if (totalPlanToAdd > 0)
                //{
                //    // Log error or handle case
                //}
            }

            if (shoppingListVM.Menu.Id == 0)
            {
                _unitOfWork.Menu.Add(shoppingListVM.Menu);
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.Menu.Update(shoppingListVM.Menu);
                _unitOfWork.Save();
            }

            bool hasErrors = false;
            string errorMessage = "";

            if (shoppingListVM.MenuRecipes != null)
            {
                foreach (var menuRecipe in shoppingListVM.MenuRecipes)
                {
                    menuRecipe.MenuId = shoppingListVM.Menu.Id;

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
                    TempData["success"] = "Products added successfully.";
                    return RedirectToAction("Index", "Home"); ; 
                }
            }
            return RedirectToAction("Index", "Home");

        }

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


    }
}
