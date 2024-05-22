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
    public class MenuRecipeRepository : Repository<MenuRecipe>, IMenuRecipeRepository
    {
        private ApplicationDbContext _db;
        public MenuRecipeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Method that updates a ProductRecipe
        /// </summary>
        /// <param name="obj">
        /// ProductRecipe to be updated
        /// </param>
        public void Update(MenuRecipe obj)
        {
            var objFromDb = _db.MenuRecipes.FirstOrDefault(s => s.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Servings = obj.Servings;
                objFromDb.RecipeId = obj.RecipeId;
                objFromDb.MenuId = obj.MenuId;
            }
        }
    }
}
