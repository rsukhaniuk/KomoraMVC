﻿using System;
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
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Number of servings")]
        public int Servings { get; set; }

        [DisplayName("Meal")]
        public int MealId { get; set; }

        [ForeignKey("MealId")]
        [ValidateNever]
        public Meal Meal { get; set; }

        [DisplayName("Recipe")]
        public int RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        [ValidateNever]
        public Recipe Recipe { get; set; }

        public Nullable<System.DateTime> Date { get; set; }

        public Nullable<bool> Status { get; set; }

    }   
       
}