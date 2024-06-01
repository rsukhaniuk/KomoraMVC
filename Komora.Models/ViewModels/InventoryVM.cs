﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.Models.ViewModels
{
    /// <summary>
    /// Viewmodel for Inventory
    /// </summary>
    public class InventoryVM
    {
        public InventoryItem InventoryItem { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> ProductList { get; set; }

    }
}
