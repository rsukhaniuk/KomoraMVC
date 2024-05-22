using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Komora.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class MealController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        public MealController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Method that returns the view with the list of units
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var MealList = _unitOfWork.Meal.GetAll(u => u.UserId == userId);

            return View(MealList);
        }

        /// <summary>
        /// Method that returns the view with the form to create or update a unit
        /// </summary>
        /// <param name="id">
        /// id of the unit to be updated
        /// </param>
        /// <returns>
        /// View with the form to create or update a unit
        /// </returns>
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                Meal mealObj = new Meal();
                mealObj.UserId = userId;
                return View(mealObj);
            }
            else
            {
                Meal mealObj = _unitOfWork.Meal.Get(u => u.Id == id);
                return View(mealObj);
            }
        }

        /// <summary>
        /// HttpPost method that creates or updates a unit
        /// </summary>
        /// <param name="obj">
        /// Unit to be created or updated
        /// </param>
        /// <param name="file">
        /// Image of unit
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Upsert(Meal obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                    obj.UserId = userId;

                    _unitOfWork.Meal.Add(obj);
                }
                else
                {
                    _unitOfWork.Meal.Update(obj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Meal created successfully";
                return RedirectToAction("Index");
            }
            else
            {
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
            var mealToBeDeleted = _unitOfWork.Meal.Get(c => c.Id == id);

            if (mealToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Meal.Remove(mealToBeDeleted);
            _unitOfWork.Save();


            return Json(new { success = true, message = "Delete Successful" });
            //return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            List<Meal> objMealList = _unitOfWork.Meal.GetAll(u => u.UserId == userId).ToList();
            return Json(new { data = objMealList });
        }
    }
}
