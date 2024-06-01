using Komora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Interface that defines the CategoryRepository
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>
        /// Method that updates a category
        /// </summary>
        /// <param name="obj"></param>
        void Update(Category obj);
    }
}
