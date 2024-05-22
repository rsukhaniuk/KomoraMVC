using Komora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Interface that defines the MenuRepository
    /// </summary>
    public interface IMenuRecipeRepository : IRepository<MenuRecipe>
    {
        void Update(MenuRecipe obj);
    }
}
