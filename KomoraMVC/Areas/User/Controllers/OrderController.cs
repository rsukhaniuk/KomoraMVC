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
    /// Controller that manages the Category model
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
        /// <param name="unitOfWork"></param>
        public OrderController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            this._unitOfWork = unitOfWork;
            this._db = db;
        }

        public List<OrderVM> CalculateOrders(double tolerance)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var today = DateTime.Today; // Define the date limit for filtering menus.

            // Fetch the necessary base data while still in IQueryable form.
            var productData = _db.Products
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    CategoryName = p.Category.Name,
                    p.Price,
                    p.Quantity,
                    PlanQuantitiesInfo = _db.ProductRecipe
                        .Join(_db.Recipes, // Join ProductRecipe with Recipes
                            pr => pr.RecipeId, // Use RecipeId from ProductRecipe as the join key
                            r => r.Id, // Use Id from Recipe as the join key
                            (pr, r) => new { ProductRecipe = pr, Recipe = r })// Project both ProductRecipe and Recipe in the result
                        .Where(joined => joined.ProductRecipe.ProductId == p.Id && joined.Recipe.UserId == userId)
                        .SelectMany(pr => _db.MenuRecipes
                            .Join(_db.Menu, // Join MenuRecipes with Menus
                                menuRecipe => menuRecipe.MenuId,
                                menu => menu.Id,
                                (menuRecipe, menu) => new { MenuRecipe = menuRecipe, Menu = menu })
                            .Where(mr => mr.MenuRecipe.RecipeId == pr.ProductRecipe.RecipeId && mr.Menu.Date >= today && mr.Menu.Status == true && mr.Menu.UserId == userId)
                            .Select(m => new { m.MenuRecipe.Servings, pr.ProductRecipe.Quantity })) // Select needed data for further in-memory processing
                        .ToList(), // Fetch data into memory here
                    Remains = _db.Inventory
                        .Where(i => i.ProductId == p.Id && i.UserId == userId)
                        .Sum(i => i.RemainQuantity)
                })
                .ToList(); // Execute the query and bring the results into memory


            // Process the calculations in memory
            var orders = productData
                .Select(p => new OrderVM
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CategoryName = p.CategoryName,
                    OrderQuan = Math.Round(Math.Ceiling((Math.Max(0, p.PlanQuantitiesInfo.Sum(x => x.Servings * x.Quantity * tolerance)
                        - Math.Max(0, p.Remains))) / p.Quantity) * p.Quantity, 3, MidpointRounding.AwayFromZero),
                    OrderPrice = (Math.Ceiling((Math.Max(0, p.PlanQuantitiesInfo.Sum(x => x.Servings * x.Quantity * tolerance)
                        - Math.Max(0, p.Remains))) / p.Quantity) * p.Price)
                })
                .Where(p => p.OrderQuan > 0)
                .ToList();
            return orders;
        }

        public IActionResult Index(string orders)
        {
            List<OrderVM> orderList = JsonConvert.DeserializeObject<List<OrderVM>>(HttpUtility.UrlDecode(orders));
            calculatedOrder = orderList;
            return View(orderList);
        }

        [HttpPost]
        public IActionResult Insert(List<OrderVM> orders)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            foreach (var item in orders)
            {
                InventoryItem inventoryItem = new InventoryItem();
                inventoryItem.UserId = userId;
                inventoryItem.ProductId = item.ProductId;
                inventoryItem.IncomeQuantity += item.OrderQuan;
                inventoryItem.IncomeDate = DateTime.Now;
                //inventoryItem.PlanQuantity += item.PlanQuan;
                //inventoryItem.PlanDate = DateTime.Now;
                //inventoryItem.RemainQuantity = item.OrderQua
                _unitOfWork.Inventory.Add(inventoryItem);
            }
            _unitOfWork.Save();

            foreach (var item in orders)
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
                        inventoryItem.PlanQuantity += toAdd;
                        inventoryItem.PlanDate = DateTime.Now;
                        inventoryItem.RemainQuantity = inventoryItem.IncomeQuantity - inventoryItem.PlanQuantity;
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

            _unitOfWork.Save();
            TempData["success"] = "Products added successfully.";
            return RedirectToAction("Index", "Home");
        }

    }
}
