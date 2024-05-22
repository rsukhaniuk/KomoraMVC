using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Komora.Models
{
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }

        

        [DisplayName("Product")]

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        [AllowNull]
        public DateTime ExpirationDate { get; set; }

        [AllowNull]
        public DateTime PlanDate { get; set; }

        [AllowNull]
        public double PlanQuantity { get; set; }

        [AllowNull]
        public DateTime IncomeDate { get; set; }

        [AllowNull]
        public double IncomeQuantity { get; set; }

        [AllowNull]
        public DateTime Remaindate { get; set; }

        [AllowNull]
        public double RemainQuantity { get; set; }

        [AllowNull]
        public DateTime WasteDate { get; set; }

        [AllowNull]
        public double WasteQuantity { get; set; }


        // Foreign Key Property
        [ValidateNever]
        public string UserId { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        [ValidateNever]
        public IdentityUser User { get; set; }



    }
}
