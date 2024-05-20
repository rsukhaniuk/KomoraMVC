using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Komora.Areas.User.Controllers
{
    /// <summary>
    /// Controller that manages the ProductRecipe model
    /// </summary>
    public class ProductRecipeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        public ProductRecipeController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Method that returns the view with the list of products
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var ProductRecipeList = _unitOfWork.ProductRecipe.GetAll(includeProperties: "Recipe,Product,Unit");
            return View(ProductRecipeList);
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
            IEnumerable<SelectListItem> RecipeList = _unitOfWork.Recipe.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            IEnumerable<SelectListItem> ProductList = _unitOfWork.Product.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            IEnumerable<SelectListItem> UnitList = _unitOfWork.Unit.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            ProductRecipeVM productRecipeVM = new ProductRecipeVM()
            {
                RecipeList = RecipeList,
                ProductList = ProductList,
                UnitList = UnitList,
                ProductRecipe = new ProductRecipe()

            };

            if (id == null || id == 0)
            {
                return View(productRecipeVM);
            }
            else
            {
                productRecipeVM.ProductRecipe = _unitOfWork.ProductRecipe.Get(u => u.Id == id);
                return View(productRecipeVM);
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
        public IActionResult Upsert(ProductRecipeVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if (obj.ProductRecipe.Id == 0)
                {
                    _unitOfWork.ProductRecipe.Add(obj.ProductRecipe);
                }
                else
                {
                    _unitOfWork.ProductRecipe.Update(obj.ProductRecipe);
                }

                _unitOfWork.Save();
                TempData["success"] = "ProductRecipe added successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                obj.RecipeList = _unitOfWork.Recipe.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                obj.ProductList = _unitOfWork.Product.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                obj.UnitList = _unitOfWork.Unit.GetAll().Select(i => new SelectListItem
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
            var productRecipeToBeDeleted = _unitOfWork.ProductRecipe.Get(u => u.Id == id);
            if (productRecipeToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            

            _unitOfWork.ProductRecipe.Remove(productRecipeToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ProductRecipe> objProductRecipeList = _unitOfWork.ProductRecipe.GetAll(includeProperties: "Recipe,Product,Unit").ToList();
            return Json(new { data = objProductRecipeList });
        }

        [HttpGet]
        public IActionResult GetAllById(int recipeId)
        {
            List<ProductRecipe> objProductRecipeList = _unitOfWork.ProductRecipe.GetAll(
                pr => pr.RecipeId == recipeId,
                includeProperties: "Recipe,Product,Unit").ToList();
            return Json(new { data = objProductRecipeList });
        }
    }
}

