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
    /// Controller that manages the Inventory model
    /// </summary>
    public class InventoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;

        /// <summary>
        /// Constructor that initializes the unitOfWork
        /// </summary>
        /// <param name="unitOfWork"></param>
        public InventoryController(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironment)
        {
            this._unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Method that returns the view with the list of products
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var Inventory = _unitOfWork.Inventory.GetAll(includeProperties: "Product");
            return View(Inventory);
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
            IEnumerable<SelectListItem> ProductList = _unitOfWork.Product.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            
            InventoryVM inventoryVM = new InventoryVM()
            {
                ProductList = ProductList,
                InventoryItem = new InventoryItem()

            };

            if (id == null || id == 0)
            {
                return View(inventoryVM);
            }
            else
            {
                inventoryVM.InventoryItem = _unitOfWork.Inventory.Get(u => u.Id == id);
                return View(inventoryVM);
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
        public IActionResult Upsert(InventoryVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {

                if (obj.InventoryItem.Id == 0)
                {
                    _unitOfWork.Inventory.Add(obj.InventoryItem);
                }
                else
                {
                    _unitOfWork.Inventory.Update(obj.InventoryItem);
                }

                _unitOfWork.Save();
                TempData["success"] = "InventoryItem added successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                obj.ProductList = _unitOfWork.Product.GetAll().Select(i => new SelectListItem
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
            var inventoryItemToBeDeleted = _unitOfWork.Inventory.Get(u => u.Id == id);
            if (inventoryItemToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            

            _unitOfWork.Inventory.Remove(inventoryItemToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<InventoryItem> objInventory = _unitOfWork.Inventory.GetAll(includeProperties: "Product").ToList();
            return Json(new { data = objInventory });
        }

    }
}

