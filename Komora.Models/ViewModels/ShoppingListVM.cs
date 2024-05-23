using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.Models.ViewModels
{
    public class ShoppingListVM
    {
        public List<OrderVM> OrderList { get; set; }
        public Menu Menu { get; set; }
        public List<MenuRecipe> MenuRecipes { get; set; }

    }
}
