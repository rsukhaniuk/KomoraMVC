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

        // List to hold calculated menus
        public List<CalculateMenuVM> CalculatedMenus { get; set; } = new List<CalculateMenuVM>();

        public Dictionary<int, string> RecipeNames { get; set; } = new Dictionary<int, string>();
    }
}
