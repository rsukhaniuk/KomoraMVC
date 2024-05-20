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
    /// Class that implements the IUnitRepository interface 
    /// </summary>
    public class UnitRepository : Repository<Unit>, IUnitRepository
    {
        private ApplicationDbContext _db;
        public UnitRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Method that updates a unit
        /// </summary>
        /// <param name="obj">
        /// unit to be updated
        /// </param>
        public void Update(Unit obj)
        {
            _db.Units.Update(obj);
        }
    }
}
