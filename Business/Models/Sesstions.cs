using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Models
{
    public class Sessions
    {
        [Key, MaxLength(50)]
        public string SessionID { get; set; }
        public int UserID { get; set; }
        [Required, MaxLength(20)]
        public string Role { get; set; }
        public DateTime ExpiresAt { get; set; }
        public virtual User User { get; set; }
    }
}
