using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }
        public List<Product> ProductList { get; set; }
        public List<Category> CategoryList { get; set; }
        public List<ProductTag> ProductTagList { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
