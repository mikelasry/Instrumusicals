using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [MinLength(5)]
        [MaxLength(64)]
        public string Email { get; set; }

        [MinLength(2)]
        [MaxLength(32)]
        public string FirstName { get; set; }

        [MinLength(2)]
        [MaxLength(32)]
        public string LastName { get; set; }

        public IEnumerable<Review> Reviews { get; set; }

        public IEnumerable<Order> Orders { get; set; }

        public string Hash { get; set; }

        public string Salt { get; set; }
    }
}
