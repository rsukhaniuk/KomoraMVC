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
    public class MenuRepository : Repository<Menu>, IMenuRepository
    {
        private ApplicationDbContext _db;
        public MenuRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Method that updates a category
        /// </summary>
        /// <param name="obj">
        /// category to be updated
        /// </param>
        public void Update(Menu obj)
        {
            var objFromDb = _db.Menu.FirstOrDefault(s => s.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Servings = obj.Servings;
                objFromDb.MealId = obj.MealId;
                objFromDb.RecipeId = obj.RecipeId;
                objFromDb.Date = obj.Date;
                objFromDb.Status = obj.Status;
            }
        }
    }
}
