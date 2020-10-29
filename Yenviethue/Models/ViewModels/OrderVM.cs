using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models.ViewModels
{
    public class OrderVM
    {
        public Order OrderHeader { get; set; }
        public List<OrderProduct> OrderProductList { get; set; }
    }
}
