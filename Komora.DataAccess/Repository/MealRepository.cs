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
    /// Class that implements the ICategoryRepository interface 
    /// </summary>
    public class MealRepository : Repository<Meal>, IMealRepository
    {
        private ApplicationDbContext _db;
        public MealRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Method that updates a category
        /// </summary>
        /// <param name="obj">
        /// category to be updated
        /// </param>
        public void Update(Meal obj)
        {
            _db.Meals.Update(obj);
        }
    }
}
