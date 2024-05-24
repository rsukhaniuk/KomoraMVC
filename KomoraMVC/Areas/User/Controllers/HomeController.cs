using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Models.ViewModels;
using Komora.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Komora.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            IEnumerable<Recipe> recipeList = _unitOfWork.Recipe.GetAll(u => u.UserId == userId, includeProperties: "Meal");
            return View(recipeList);
        }

        public IActionResult Details(int recipeId)
        {
            RecipeVM recipeVM = new RecipeVM()
            {
                Recipe = _unitOfWork.Recipe.Get(u => u.Id == recipeId, includeProperties: "Meal"),
                ProductRecipes = _unitOfWork.ProductRecipe.GetAll(u => u.RecipeId == recipeId, includeProperties: "Recipe,Product,Unit").ToList(),
            };

            
            return View(recipeVM);
        }

        

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
