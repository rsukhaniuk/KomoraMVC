﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.Models.ViewModels
{
    /// <summary>
    /// Menu View Model
    /// </summary>
    public class MenuVM
    {
        public Menu Menu { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> MealList { get; set; }

        [ValidateNever]
        public Dictionary<Meal, IEnumerable<SelectListItem>> RecipeMealDict { get; set; }

        [ValidateNever]
        public List<MenuRecipe> MenuRecipes { get; set; }


        // Other properties
        public bool StatusForDisplay
        {
            get => Status ?? true; // Default to false or true depending on your logic
            set => Status = value;
        }

        public bool? Status { get; set; }
    }
}
