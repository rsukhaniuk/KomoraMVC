using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Komora.Models
{
    public class ProductRecipe
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Recipe")]
        public int RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        [ValidateNever]
        public Recipe Recipe { get; set; }

        [DisplayName("Product")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        [DisplayName("Unit")]
        public int UnitId { get; set; }

        [ForeignKey("UnitId")]
        [ValidateNever]
        public Unit Unit { get; set; }



    }   
       
}
