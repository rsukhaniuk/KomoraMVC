using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.Models.ViewModels
{
    public class MenuPlanningViewModel
    {
        // Parameters for calculating the menu
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ServingsPerMeal { get; set; }

        public int? TotalCalories { get; set; }  // Optional: total calories for the entire menu
        public bool IsVegan { get; set; }  // Indicates if the entire menu is vegan

        // List to hold calculated menus
        public List<CalculateMenuVM> CalculatedMenus { get; set; } = new List<CalculateMenuVM>();

        public Dictionary<int, (string MealName, string RecipeName)> RecipeNames { get; set; } = new Dictionary<int, (string, string)>();
    }
}
