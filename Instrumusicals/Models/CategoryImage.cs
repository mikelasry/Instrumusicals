using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class CategoryImage
    {
        public int Id { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [Required]
        [NotMapped]
        [Display(Name = "Image File")]
        public IFormFile ImageFile { get; set; }
        public byte[] Image { get; set; }
    }
}
