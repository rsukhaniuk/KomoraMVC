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
    public class Recipe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Recipe Name")]
        public string Name { get; set; }


        [DisplayName("Cooking Time")]
        public string CookingTime { get; set; }


        [DisplayName("Preparation details")]
        public string Preparation { get; set; }

        [ValidateNever]
        [DisplayName("Image")]
        public string? imgUrl { get; set; }


        [DisplayName("Meal")]
        [Required]
        public int MealId { get; set; }

        [ForeignKey("MealId")]
        [ValidateNever]
        public Meal Meal { get; set; }

        [ValidateNever]
        public string UserId { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        [ValidateNever]
        public IdentityUser User { get; set; }

        // Optional fields
        [DisplayName("Calories")]
        public int? Calories { get; set; }  // Nullable int for Calories

        [DisplayName("Vegetarian")]
        public bool IsVegetarian { get; set; } = false;  // Default value set to false

    }   
       
}
