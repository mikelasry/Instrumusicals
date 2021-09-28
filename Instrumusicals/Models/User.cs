using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public enum UserType
    {
        Client = 1001,
        Author,
        Editor,
        Admin
    }
    
    
    public class User
    {
        
        public int Id { get; set; }
        
        [Required]
        [MaxLength(64)]
        [MinLength(5)]
        public string Email { get; set; }

        [MinLength(2)]
        [MaxLength(32)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [MinLength(2)]
        [MaxLength(32)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(128)]
        public string Address { get; set; }

        [Required]
        [NotMapped]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Hash { get; set; }

        public string Salt { get; set; }

        public UserType UserType { get; set; } = UserType.Client;

        // ------------------------------------------------------
        // ------------------------------------------------------

        public IEnumerable<Review> Reviews { get; set; }

        public IEnumerable<Order> Orders { get; set; }
    }
}
