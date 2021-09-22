using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Instrumusicals.Models;

namespace Instrumusicals.Data
{
    public class InstrumusicalsContext : DbContext
    {
        public InstrumusicalsContext (DbContextOptions<InstrumusicalsContext> options)
            : base(options)
        {
        }

        public DbSet<Instrumusicals.Models.Category> Category { get; set; }

        public DbSet<Instrumusicals.Models.CategoryImage> CategoryImage { get; set; }

        public DbSet<Instrumusicals.Models.Instrument> Instrument { get; set; }

        public DbSet<Instrumusicals.Models.Order> Order { get; set; }

        public DbSet<Instrumusicals.Models.Review> Review { get; set; }

        public DbSet<Instrumusicals.Models.User> User { get; set; }
    }
}
