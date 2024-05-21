using Komora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Interface that defines the IMealRepository
    /// </summary>
    public interface IMealRepository : IRepository<Meal>
    {
        void Update(Meal obj);
    }
}
