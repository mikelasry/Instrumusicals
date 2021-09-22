using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class CategoryImage
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }
        public byte[] Image { get; set; }
    }
}
