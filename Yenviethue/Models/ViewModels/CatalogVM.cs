using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models.ViewModels
{
    public class CatalogVM
    {
        public List<Category> CategoryList { get; set; }
        public Dictionary<int, List<SubCategory>> RepoSubCategoryList { get; set; }

        public Category CategoryCurrent { get; set; }
        public SubCategory SubCategoryCurrent { get; set; }

        public List<Product> ProductList { get; set; }
    }
}
