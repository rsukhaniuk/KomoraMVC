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
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Product Name")]
        public string Name { get; set; }

        [DisplayName("Category")]

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

        [Required]
        public double Quantity { get; set; }

        [DisplayName("Unit")]

        public int UnitId { get; set; }

        [ForeignKey("UnitId")]
        [ValidateNever]
        public Unit Unit { get; set; }

        [Required]
        public double Price { get; set; }

        [ValidateNever]
        [DisplayName("Image")]
        public string? imgUrl { get; set; }


    }
}
