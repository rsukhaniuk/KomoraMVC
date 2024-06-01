using Komora.DataAccess.Repository;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using Komora.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting.Internal;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Komora.Areas.User.Controllers
{
    /// <summary>
    /// Controller that manages the Inventory model
    /// </summary>
    [Area("User")]
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;

        /// <summary>
        /// Constructor that initializes the unitOfWork and the hostingEnvironment
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="hostingEnvironment"></param>
        public InventoryController(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironment)
        {
            this._unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Method that returns the view with the list of inventory items
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var Inventory = _unitOfWork.Inventory.GetAll(u => u.UserId == userId, includeProperties: "Product");
            
            return View(Inventory);
        }

        /// <summary>
        /// Method that returns the view with the form to create or update a inventory item
        /// </summary>
        /// <param name="id">
        /// id of the inventory item to be updated or created
        /// </param>
        /// <returns>
        /// View with the form to create or update a inventory item
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
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                inventoryVM.InventoryItem.UserId = userId;

                return View(inventoryVM);
            }
            else
            {
                inventoryVM.InventoryItem = _unitOfWork.Inventory.Get(u => u.Id == id);

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                inventoryVM.InventoryItem.UserId = userId;

                return View(inventoryVM);
            }
        }

        /// <summary>
        /// HttpPost method that creates or updates a inventory item
        /// </summary>
        /// <param name="obj">
        /// inventory item to be created or updated
        /// </param>
        /// <param name="file">
        /// Image of inventory item
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Upsert(InventoryVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {

                if (obj.InventoryItem.Id == 0)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                    obj.InventoryItem.UserId = userId;

                    _unitOfWork.Inventory.Add(obj.InventoryItem);
                }
                else
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                    obj.InventoryItem.UserId = userId;

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
        /// HttpDelete method that deletes a inventory item
        /// </summary>
        /// <param name="id">id of the inventory item to be deleted</param>
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

        /// <summary>
        /// HttpGet method that returns all the inventory items in JSON format
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<InventoryItem> objInventory = _unitOfWork.Inventory.GetAll(u => u.UserId == userId, includeProperties: "Product").ToList();

            return Json(new { data = objInventory });
        }

    }
}

