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

    }   
       
}
