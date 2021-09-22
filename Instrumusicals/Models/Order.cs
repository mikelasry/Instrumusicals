using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        [Display(Name = "Owner")]
        public User User { get; set; }

        public IEnumerable<Instrument> Instruments { get; set; }

        [MinLength(6)]
        [MaxLength(64)]
        public string Address { get; set; }

        [Range(0.0, float.MaxValue, ErrorMessage = "{0} field must be between {1} and {2}")]
        public float TotalPrice { get; set; }

        public DateTime Create { get; set; } = DateTime.Now;

        public DateTime LastUpdate { get; set; }
    }
}
