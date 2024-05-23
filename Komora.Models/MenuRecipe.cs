using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Identity;

namespace Komora.Models
{
    public class MenuRecipe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Number of servings")]
        public int Servings { get; set; }

        

        [DisplayName("Recipe")]
        public int RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        [ValidateNever]
        [Required]
        public Recipe Recipe { get; set; }


        [DisplayName("Menu")]
        public int MenuId { get; set; }

        [ForeignKey("MenuId")]
        [ValidateNever]
        [Required]
        public Menu Menu { get; set; }



    }

}
