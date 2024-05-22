using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Komora.Models
{
    public class Meal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Meal Name")]
        [MaxLength(50)]
        public string Name { get; set; }

        // Foreign Key Property
        [ValidateNever]
        public string UserId { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        [ValidateNever]
        public IdentityUser User { get; set; }
    }
}
