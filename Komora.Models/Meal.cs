using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
