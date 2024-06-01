using Komora.DataAccess.Data;
using Komora.DataAccess.Repository.IRepository;
using Komora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.DataAccess.Repository
{
    /// <summary>
    /// Class that implements the IInventoryRepository interface 
    /// </summary>
    public class InventoryRepository : Repository<InventoryItem>, IInventoryRepository
    {
        private ApplicationDbContext _db;
        public InventoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Method that updates an inventory item
        /// </summary>
        /// <param name="obj">
        /// inventory item to be updated
        /// </param>
        public void Update(InventoryItem obj)
        {
            var objFromDb = _db.Inventory.FirstOrDefault(s => s.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.ProductId = obj.ProductId;
                objFromDb.ExpirationDate = obj.ExpirationDate;
                objFromDb.PlanDate = obj.PlanDate;
                objFromDb.PlanQuantity = obj.PlanQuantity;
                objFromDb.IncomeDate = obj.IncomeDate;
                objFromDb.IncomeQuantity = obj.IncomeQuantity;
                objFromDb.Remaindate = obj.Remaindate;
                objFromDb.RemainQuantity = obj.RemainQuantity;
                objFromDb.WasteDate = obj.WasteDate;
                objFromDb.WasteQuantity = obj.WasteQuantity;
                objFromDb.UserId = obj.UserId;
            }
        }
    }
}
