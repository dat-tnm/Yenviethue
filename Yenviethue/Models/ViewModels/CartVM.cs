using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models.ViewModels
{
    public class CartVM
    {
        public Order OrderHeader { get; set; }
        public ShippingInfo ShippingInfo { get; set; }
        public List<ShoppingCart> CartList { get; set; }
    }
}
