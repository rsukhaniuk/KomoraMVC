﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.DataAccess.Repository.IRepository
{
    /// <summary>
    /// Interface that defines the Unit of Work pattern
    /// </summary>
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IUnitRepository Unit { get; }
        IProductRepository Product { get; }
        IRecipeRepository Recipe { get; }
        IProductRecipeRepository ProductRecipe { get; }
        IMealRepository Meal { get; }
        IMenuRepository Menu { get; }
        IMenuRecipeRepository MenuRecipe { get; }

        IInventoryRepository Inventory { get; }

        IOrderItemRepository OrderItem { get; }
        void Save();
    }
}
