using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models
{
    public class ShippingInfo
    {
        public int Id { get; set; }


        public string UserId { get; set; }

        [NotMapped]
        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }




        [Required]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        public string Province { get; set; }

        [Required]
        public string Address { get; set; }
    }
}
