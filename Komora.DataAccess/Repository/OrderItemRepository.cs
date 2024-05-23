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
    /// Class that implements the IMenuRepository interface 
    /// </summary>
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        private ApplicationDbContext _db;
        public OrderItemRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Method that updates a category
        /// </summary>
        /// <param name="obj">
        /// category to be updated
        /// </param>
        public void Update(OrderItem obj)
        {
            var objFromDb = _db.OrderItems.FirstOrDefault(s => s.Id == obj.Id);
            if (objFromDb != null)
            {
               
                objFromDb.ProductId = obj.ProductId;
                //objFromDb.ProductName = obj.ProductName;
                //objFromDb.CategoryName = obj.CategoryName;
                objFromDb.OrderQuan = obj.OrderQuan;
                objFromDb.OrderPrice = obj.OrderPrice;
                objFromDb.Date = obj.Date;
                objFromDb.UserId = obj.UserId;
            }
        }
    }
}
