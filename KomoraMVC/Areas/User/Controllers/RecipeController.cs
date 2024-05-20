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
    /// Controller that manages the Recipe model
    /// </summary>
    public class RecipeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        public RecipeController(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironment)
        {
            this._unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Method that returns the view with the list of recipes
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var RecipeList = _unitOfWork.Recipe.GetAll();
            return View(RecipeList);
        }

        /// <summary>
        /// Method that returns the view with the form to create or update a recipe
        /// </summary>
        /// <param name="id">
        /// id of the recipe to be updated
        /// </param>
        /// <returns>
        /// View with the form to create or update a recipe
        /// </returns>
        public IActionResult Upsert(int? id)
        {
            RecipeVM recipeVM = new RecipeVM
            {
                ProductList = _unitOfWork.Product.GetAll().Select(p => new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                }),
                UnitList = _unitOfWork.Unit.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Recipe = new Recipe(),
                ProductRecipes = new List<ProductRecipe>()
            };


            if (id == null || id == 0)
            {
                return View(recipeVM);
            }
            else
            {
                recipeVM.Recipe = _unitOfWork.Recipe.Get(u => u.Id == id);
                recipeVM.ProductRecipes = _unitOfWork.ProductRecipe.GetAll(pr => pr.RecipeId == id).ToList();
                return View(recipeVM);
            }
        }

        /// <summary>
        /// HttpPost method that creates or updates a recipe
        /// </summary>
        /// <param name="obj">
        /// recipe to be created or updated
        /// </param>
        /// <param name="file">
        /// Image of recipe
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Upsert(RecipeVM recipeVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostingEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string recipePath = Path.Combine(wwwRootPath + @"\images\recipe", fileName);

                    if (!string.IsNullOrEmpty(recipeVM.Recipe.imgUrl))
                    {
                        string oldFilePath = Path.Combine(wwwRootPath + recipeVM.Recipe.imgUrl);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    using (var fileStream = new FileStream(recipePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    recipeVM.Recipe.imgUrl = @"\images\recipe\" + fileName;
                }

                if (recipeVM.Recipe.Id == 0)
                {
                    _unitOfWork.Recipe.Add(recipeVM.Recipe);
                }
                else
                {
                    _unitOfWork.Recipe.Update(recipeVM.Recipe);
                }

                _unitOfWork.Save();
                TempData["success"] = "Recipe added successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                return View(recipeVM);

            }
        }

        /// <summary>
        /// HttpDelete method that deletes a recipe
        /// </summary>
        /// <param name="id">id of the recipe to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var recipeToBeDeleted = _unitOfWork.Recipe.Get(u => u.Id == id);
            if (recipeToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath + recipeToBeDeleted.imgUrl);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            _unitOfWork.Recipe.Remove(recipeToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Recipe> objRecipeList = _unitOfWork.Recipe.GetAll().ToList();
            return Json(new { data = objRecipeList });
        }
    }
}

