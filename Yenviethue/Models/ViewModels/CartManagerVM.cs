using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models.ViewModels
{
    public class CartManagerVM
    {
        public List<ShoppingCart> CartList { get; set; }
        public List<Guest> GuestList { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
