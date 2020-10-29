using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Yenviethue.Models
{
    public class Guest
    {
        public Guest()
        {
            ExpirationDate = DateTime.Now.Date.AddDays(29);
        }

        public string Id { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }
    }
}
