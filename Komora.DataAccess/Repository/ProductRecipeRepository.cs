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
    public class ProductRecipeRepository : Repository<ProductRecipe>, IProductRecipeRepository
    {
        private ApplicationDbContext _db;
        public ProductRecipeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Method that updates a ProductRecipe
        /// </summary>
        /// <param name="obj">
        /// ProductRecipe to be updated
        /// </param>
        public void Update(ProductRecipe obj)
        {
            var objFromDb = _db.ProductRecipe.FirstOrDefault(s => s.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.RecipeId = obj.RecipeId;
                objFromDb.ProductId = obj.ProductId;
                objFromDb.Quantity = obj.Quantity;
                objFromDb.UnitId = obj.UnitId;
            }
        }
    }
}
