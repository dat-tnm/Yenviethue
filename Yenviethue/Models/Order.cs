using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        [NotMapped]
        [ForeignKey("UserId")]
        public ApplicationUser ApplicationUser { get; set; }


        public int ShipmentId { get; set; }

        [ForeignKey("ShipmentId")]
        public virtual ShippingInfo ShippingInfo { get; set; }



        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public double OrderTotalOriginal { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public double OrderTotal { get; set; }



        public double ShippingFee { get; set; }
        public string CouponCode { get; set; }
        public double CouponDiscount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string Comments { get; set; }

        public string TransactionId { get; set; }

        public enum EPaymentMethod { COD = 0, VISA = 1 };
        public int PaymentMethod { get; set; }
    }
}
