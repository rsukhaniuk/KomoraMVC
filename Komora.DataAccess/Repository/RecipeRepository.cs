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
    /// Class that implements the IRecipeRepository interface 
    /// </summary>
    public class RecipeRepository : Repository<Recipe>, IRecipeRepository
    {
        private ApplicationDbContext _db;
        public RecipeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Method that updates a recipe
        /// </summary>
        /// <param name="obj">
        /// recipe to be updated
        /// </param>
        public void Update(Recipe obj)
        {
            var objFromDb = _db.Recipes.FirstOrDefault(s => s.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = obj.Name;
                objFromDb.CookingTime = obj.CookingTime;
                objFromDb.Preparation = obj.Preparation;
                objFromDb.MealId = obj.MealId;
                objFromDb.IsVegetarian = obj.IsVegetarian;
                objFromDb.Calories = obj.Calories;

                if (obj.imgUrl != null)
                {
                    objFromDb.imgUrl = obj.imgUrl;
                }
            }
        }
    }
}
