using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.Models.ViewModels
{
    /// <summary>
    /// ViewModel for ProductRecipe
    /// </summary>
    public class ProductRecipeVM
    {
        public ProductRecipe ProductRecipe { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> RecipeList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> ProductList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> UnitList { get; set; }
    }
}
