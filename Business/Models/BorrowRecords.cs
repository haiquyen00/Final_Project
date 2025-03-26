using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Models
{
    public class BorrowRecords
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int UserID { get; set; }
        public int BookID { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool Returned { get; set; } = false;

        // Notification tracking
        public bool ReminderSent { get; set; } = false;
        public DateTime? LastReminderDate { get; set; }
        public int ReminderCount { get; set; } = 0;

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Books Book { get; set; }
    }
}
