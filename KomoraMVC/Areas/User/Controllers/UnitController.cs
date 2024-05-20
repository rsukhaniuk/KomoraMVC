using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Microsoft.AspNetCore.Mvc;

namespace Komora.Areas.User.Controllers
{
    public class UnitController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        public UnitController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Method that returns the view with the list of units
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var UnitList = _unitOfWork.Unit.GetAll();
            return View(UnitList);
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
                return View(new Unit());
            }
            else
            {
                Unit unitObj = _unitOfWork.Unit.Get(u => u.Id == id);
                return View(unitObj);
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
        public IActionResult Upsert(Unit obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.Unit.Add(obj);
                }
                else
                {
                    _unitOfWork.Unit.Update(obj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Unit created successfully";
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
            var unitToBeDeleted = _unitOfWork.Unit.Get(c => c.Id == id);

            if (unitToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Unit.Remove(unitToBeDeleted);
            _unitOfWork.Save();


            return Json(new { success = true, message = "Delete Successful" });
            //return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Unit> objUnitList = _unitOfWork.Unit.GetAll().ToList();
            return Json(new { data = objUnitList });
        }
    }
}
