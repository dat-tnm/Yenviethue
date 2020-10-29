using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models.ViewModels
{
    public class HomeVM
    {
        public List<Product> ListTop4HotProduct { get; set; }
        public List<Product> ListTop4NewProduct { get; set; }
    }
}
