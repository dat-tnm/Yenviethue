using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models.ViewModels
{
    public class OrderListVM
    {
        public List<OrderVM> OrderList { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
