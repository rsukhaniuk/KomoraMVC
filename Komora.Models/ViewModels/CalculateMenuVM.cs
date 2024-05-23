using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.Models.ViewModels
{
    public class CalculateMenuVM
    {
        public Menu Menu { get; set; }
        public List<MenuRecipe> MenuRecipes { get; set; }
        public double TotalCost { get; set; }
        public bool CanPrepare { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ServingsPerMeal { get; set; }


    }
}
