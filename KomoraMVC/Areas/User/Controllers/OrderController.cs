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
            var today = DateTime.Today;

            var products = _db.Products
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    CategoryName = p.Category.Name,
                    p.Price,
                    PlanQuantities = _db.ProductRecipe
                        .Where(pr => pr.ProductId == p.Id)
                        .SelectMany(pr => _db.Menu
                            .Where(m => m.RecipeId == pr.RecipeId && m.Date >= today && m.Status == false)
                            .Select(m => new { m.Servings, pr.Quantity }))
                        .Sum(x => x.Servings * x.Quantity * 1.2), // Including 20% tolerance and x servings
                    Remains = _db.Inventory
                        .Where(i => i.ProductId == p.Id)
                        .Sum(i => i.RemainQuantity)
                })
                .Select(p => new OrderVM
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CategoryName = p.CategoryName,
                    OrderQuan = Math.Ceiling(Math.Max(0, p.PlanQuantities - p.Remains) / p.Price) * p.Price,
                    OrderPrice = Math.Ceiling(Math.Max(0, p.PlanQuantities - p.Remains) / p.Price) * p.Price * p.Price
                })
                .Where(p => p.OrderQuan > 0);

            return (List<OrderVM>)products;

        }

        public IActionResult Index()
        {
            return View(CalculateOrders());
        }

    }
}
