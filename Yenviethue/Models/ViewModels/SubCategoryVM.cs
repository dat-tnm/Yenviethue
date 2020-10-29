using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models.ViewModels
{
    public class SubCategoryVM
    {
        public List<Category> CategoryList { get; set; }
        public List<SubCategory> SubCategoryList { get; set; }
        public SubCategory SubCategory { get; set; }
        public string StatusMessage { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
