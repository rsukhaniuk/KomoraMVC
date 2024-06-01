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
    /// <summary>
    /// Model that represents the Menu
    /// </summary>
    public class Menu
    {
        [Key]
        public int Id { get; set; }


        //[Required]
        public Nullable<System.DateTime> Date { get; set; }

        public Nullable<bool> Status { get; set; }

        // Foreign Key Property
        [ValidateNever]
        public string UserId { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        [ValidateNever]
        public IdentityUser User { get; set; }

    }   
       
}
