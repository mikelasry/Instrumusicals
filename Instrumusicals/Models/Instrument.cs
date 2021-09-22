using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class Instrument
    {
        public int Id { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(64)]
        public string Name { get; set; }

        [MinLength(2)]
        [MaxLength(64)]
        public string Brand { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [NotMapped] // The file uploaded should be converted to bytes array and NOT be MAPPED to the db.
        public IFormFile ImageFile { get; set; }
        public byte[] Image { get; set; } // Actual image as bytes array

        [MinLength(2)]
        [MaxLength(512)]
        public string Description { get; set; }

        public IEnumerable<Review> Reviews { get; set; }
        public IEnumerable<Order> Orders { get; set; }

        [Range( 0, int.MaxValue, ErrorMessage = "{0} field must be between {1} and {2}" )]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "{0} field must be between {1} and {2}")]
        public int Sold { get; set; }

        [Range(1.0, float.MaxValue, ErrorMessage = "{0} field must be between {1} and {2}")]
        public float Price { get; set; }

    }

}