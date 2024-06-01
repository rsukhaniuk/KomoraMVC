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
    /// Model that defines the OrderItem
    /// </summary>
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }


        [ValidateNever]
        public int ProductId { get; set; }

        // Navigation Property
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        //public string ProductName { get; set; }
        //public string CategoryName { get; set; }
        public double OrderQuan { get; set; }
        public double OrderPrice { get; set; }
        
        public Nullable<System.DateTime> Date { get; set; }

        [ValidateNever]
        public string UserId { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        [ValidateNever]
        public IdentityUser User { get; set; }

    }   
       
}
