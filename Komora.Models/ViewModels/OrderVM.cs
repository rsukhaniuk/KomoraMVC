using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.Models.ViewModels
{
    /// <summary>
    /// ViewModel for Order
    /// </summary>
    public class OrderVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public double OrderQuan { get; set; }
        public string Unit { get; set; }
        public double OrderPrice { get; set; }
        public double PlanQuan { get; set; }
    }
}
