using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.Models.ViewModels
{
    public class MenuVM
    {
        public Menu Menu { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> MealList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> RecipeList { get; set; }

        // Other properties
        public bool StatusForDisplay
        {
            get => Status ?? false; // Default to false or true depending on your logic
            set => Status = value;
        }

        public bool? Status { get; set; }
    }
}
