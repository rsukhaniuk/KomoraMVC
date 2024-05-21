using Komora.DataAccess.Data;
using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Komora.Areas.User.Controllers
{
    /// <summary>
    /// Controller that manages the Category model
    /// </summary>
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private List<OrderVM> calculatedOrder;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        public OrderController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            this._unitOfWork = unitOfWork;
            this._db = db;
        }

        public List<OrderVM> CalculateOrders()
        {
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
                        .Where(pr => pr.ProductId == p.Id)
                        .SelectMany(pr => _db.Menu
                            .Where(m => m.RecipeId == pr.RecipeId && m.Date >= today && m.Status == true)
                            .Select(m => new { m.Servings, pr.Quantity })) // Select needed data for further in-memory processing
                        .ToList(), // Fetch data into memory here
                    Remains = _db.Inventory
                        .Where(i => i.ProductId == p.Id)
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
                    OrderQuan = Math.Round(Math.Ceiling((Math.Max(0, p.PlanQuantitiesInfo.Sum(x => x.Servings * x.Quantity * 1.2)
                        - Math.Max(0, p.Remains))) / p.Quantity) * p.Quantity, 3, MidpointRounding.AwayFromZero),
                    OrderPrice = (Math.Ceiling((Math.Max(0, p.PlanQuantitiesInfo.Sum(x => x.Servings * x.Quantity * 1.2)
                        - Math.Max(0, p.Remains))) / p.Quantity) * p.Price)
                })
                .Where(p => p.OrderQuan > 0)
                .ToList();  

            //orders.ForEach(o =>
            //{
            //    var product = productData.First(p => p.Id == o.ProductId);
            //    o.OrderPrice = o.OrderQuan * product.Price;
            //});

            return orders;

        }

        public IActionResult Index()
        {
            return View(CalculateOrders());
        }

        [HttpPost]
        public IActionResult Insert()
        {
            var orders = CalculateOrders();
            foreach (var item in orders)
            {
                InventoryItem inventoryItem = new InventoryItem();
                inventoryItem.ProductId = item.ProductId;
                inventoryItem.RemainQuantity = item.OrderQuan;
                inventoryItem.IncomeDate = DateTime.Now;
                _unitOfWork.Inventory.Add(inventoryItem);
            }

            _unitOfWork.Save();
            TempData["success"] = "Products added successfully.";
            return RedirectToAction("Index");
        }

    }
}
