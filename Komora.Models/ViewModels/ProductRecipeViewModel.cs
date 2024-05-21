using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.Models.ViewModels
{
    public class ProductRecipeViewModel
    {
        public int ProductId { get; set; }
        public int RecipeId { get; set; }
        public decimal Quantity { get; set; }
        public List<Menu> Menus { get; set; }
    }
}
