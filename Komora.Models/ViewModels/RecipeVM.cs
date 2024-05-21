using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.Models.ViewModels
{
    public class RecipeVM
    {
        public Recipe Recipe { get; set; }

        //public IEnumerable<SelectListItem> ProductRecipeList { get; set; }
        public List<ProductRecipe> ProductRecipes { get; set; }
        public IEnumerable<SelectListItem> ProductList { get; set; }
        public IEnumerable<SelectListItem> UnitList { get; set; }
    }
}
