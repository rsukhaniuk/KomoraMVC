using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Komora.Areas.User.Controllers
{
    /// <summary>
    /// Controller that manages the Recipe model
    /// </summary>
    [Area("User")]
    [Authorize]
    public class RecipeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private List<ProductRecipe> productRecipes;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        public RecipeController(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironment)
        {
            this._unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
            productRecipes = new List<ProductRecipe>();
        }

        /// <summary>
        /// Method that returns the view with the list of recipes
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var RecipeList = _unitOfWork.Recipe.GetAll(u => u.UserId == userId, includeProperties: "Meal");
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
            RecipeVM recipeVM;
            string sessionData = HttpContext.Session.GetString("RecipeData");
            if (!string.IsNullOrEmpty(sessionData))
            {
                recipeVM = JsonConvert.DeserializeObject<RecipeVM>(sessionData);
                HttpContext.Session.Remove("RecipeData"); // Clear session after retrieval
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                Recipe recipeObj = new Recipe();
                recipeObj.UserId = userId;

                recipeVM = new RecipeVM
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
                    MealList = _unitOfWork.Meal.GetAll(u => u.UserId == userId).Select(m => new SelectListItem
                    {
                        Text = m.Name,
                        Value = m.Id.ToString()
                    }),
                    Recipe = new Recipe(),

                    ProductRecipes = new List<ProductRecipe>()
                };
            }



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

        public IActionResult SaveRecipeToSession(RecipeVM model)
        {
            var recipeJson = JsonConvert.SerializeObject(model);
            HttpContext.Session.SetString("RecipeData", recipeJson);

            return RedirectToAction("UpsertIngridient");
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
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                recipeVM.Recipe.UserId = userId;


                _unitOfWork.Recipe.Add(recipeVM.Recipe);
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.Recipe.Update(recipeVM.Recipe);
                _unitOfWork.Save();
            }

            bool hasErrors = false;
            string errorMessage = "";

            if (recipeVM.ProductRecipes != null)
            {
                foreach (var productRecipe in recipeVM.ProductRecipes)
                {
                    productRecipe.RecipeId = recipeVM.Recipe.Id;

                    if (string.IsNullOrWhiteSpace(productRecipe.ProductId.ToString()) || productRecipe.ProductId == 0)
                    {
                        errorMessage += "Product is required. ";
                        hasErrors = true;
                    }

                    // Припускаємо, що UnitId також є обов'язковим
                    if (string.IsNullOrWhiteSpace(productRecipe.UnitId.ToString()) || productRecipe.UnitId == 0)
                    {
                        errorMessage += "Unit is required. ";
                        hasErrors = true;
                    }

                    // Якщо немає помилок, додаємо або оновлюємо записи
                    if (!hasErrors)
                    {
                        if (productRecipe.Id == 0)
                        {
                            _unitOfWork.ProductRecipe.Add(productRecipe);
                        }
                        else
                        {
                            _unitOfWork.ProductRecipe.Update(productRecipe);
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
                    TempData["success"] = "Recipe added successfully.";
                    return RedirectToAction("Index"); // Переходимо до списку рецептів, якщо все в порядку
                }
            }
            return RedirectToAction("Index");

            //if (recipeVM.ProductRecipes != null)
            //    foreach (var productRecipe in recipeVM.ProductRecipes)
            //    {
            //        productRecipe.RecipeId = recipeVM.Recipe.Id; // Ensure the RecipeId is set

            //        if (string.IsNullOrWhiteSpace(productRecipe.ProductId.ToString()) || productRecipe.ProductId == 0)
            //        {
            //            errorMessage += "Product ID is required. ";
            //            hasErrors = true;
            //        }

            //        if (!hasErrors)
            //        {
            //            if (productRecipe.Id == 0)
            //            {
            //                _unitOfWork.ProductRecipe.Add(productRecipe);
            //            }
            //            else
            //            {
            //                _unitOfWork.ProductRecipe.Update(productRecipe);
            //            }
            //        }
            //    }
            //if (!hasErrors)
            //{
            //    _unitOfWork.Save();
            //    TempData["success"] = "Recipe added successfully.";
            //}

            return RedirectToAction("Index");
        }


        public IActionResult UpsertIngridient(int? id)
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


        [HttpPost]
        public IActionResult UpsertIngridient(ProductRecipeVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if (obj.ProductRecipe.Id == 0)
                {
                    //_unitOfWork.ProductRecipe.Add(obj.ProductRecipe);
                    productRecipes.Add(obj.ProductRecipe);
                }
                else
                {
                    //_unitOfWork.ProductRecipe.Update(obj.ProductRecipe);
                    productRecipes.Add(obj.ProductRecipe);
                }

                //_unitOfWork.Save();
                //TempData["success"] = "ProductRecipe added successfully.";

                return RedirectToAction("Upsert");
                //string referer = Request.Headers["Referer"].ToString();
                //if (!string.IsNullOrEmpty(referer))
                //{
                //    return Redirect(referer);  // Redirect to the previous page
                //}
                //else
                //{
                //    return RedirectToAction("Index");  // Fallback to the index action if referer is not available
                //}
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

        [HttpDelete]
        public IActionResult DeleteIngridient(int? id)
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
        public IActionResult GetAllIngridients()
        {
            List<ProductRecipe> objProductRecipeList = _unitOfWork.ProductRecipe.GetAll(includeProperties: "Recipe,Product,Unit").ToList();
            return Json(new { data = objProductRecipeList });
        }

        [HttpGet]
        public IActionResult GetAllIngridientsById(int recipeId)
        {
            List<ProductRecipe> objProductRecipeList = _unitOfWork.ProductRecipe.GetAll(
                pr => pr.RecipeId == recipeId,
                includeProperties: "Recipe,Product,Unit").ToList();
            return Json(new { data = objProductRecipeList });
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<Recipe> objRecipeList = _unitOfWork.Recipe.GetAll(u => u.UserId == userId, includeProperties: "Meal").ToList();
            return Json(new { data = objRecipeList });
        }
    }
}

