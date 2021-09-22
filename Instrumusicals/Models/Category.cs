using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class Category
    {
        public int Id { get; set; }

        [MinLength(2)]
        [MaxLength(64)]
        public string Name { get; set; }

        public IEnumerable<Instrument> Instruments { get; set; }

        [Display(Name = "Category Image")]
        public CategoryImage CategoryImage { get; set; }
    }
}
