using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int InstrumentId { get; set; }
        public Instrument Instrument { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; }

        [MinLength(2)]
        [MaxLength(512)]
        public string Content{ get; set; }
    }
}
