using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models
{
    public partial class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock should be 0 or greater")]
        public int StockQuantity { get; set; }

        [Range(1, int.MaxValue, ErrorMessage ="Price should be greater than {1} (currency: thousand VND)")]
        public double Price { get; set; }

        [DisplayName("(Mô tả ngắn) Quy cách đóng gói")]
        public string DescriptionSummary { get; set; }

        public string DescriptionDetails { get; set; }

        public string ImgSrc { get; set; }



        [DisplayName("SubCategory")]
        public int SubCategoryId { get; set; }

        [ForeignKey("SubCategoryId")]
        public virtual SubCategory SubCategory { get; set; }


        public virtual ICollection<ProductTag> ProductTags { get; set; }
    }
}
