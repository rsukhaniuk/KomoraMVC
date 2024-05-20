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
    /// Class that implements the IProductRepository interface 
    /// </summary>
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Method that updates a product
        /// </summary>
        /// <param name="obj">
        /// product to be updated
        /// </param>
        public void Update(Product obj)
        {
            var objFromDb = _db.Products.FirstOrDefault(s => s.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = obj.Name;
                objFromDb.CategoryId = obj.CategoryId;
                objFromDb.Quantity = obj.Quantity;
                objFromDb.UnitId = obj.UnitId;
                objFromDb.Price = obj.Price;
                if (obj.imgUrl != null)
                {
                    objFromDb.imgUrl = obj.imgUrl;
                }
            }
        }
    }
}
