using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Instrumusicals.Models
{
    public class userOrderModel
    {
        public userOrderModel(string userFName, string userLName, float price, DateTime shipping)
        {
            UserFName = userFName;
            UserLName = userLName;
            Price = price;
            Shipping = shipping;
        }

        public string UserFName { get; set; }

        public string UserLName { get; set; }

        public float Price { get; set; }

        public DateTime Shipping { get; set; }
        

    }
}
