using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class Store
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }
        
        public float Lat { get; set; }
        public float Lng { get; set; }
    }
}
