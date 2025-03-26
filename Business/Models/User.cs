using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Models
{

    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        public User()
        {
            BorrowRecords = new HashSet<BorrowRecords>();
            Sessions = new HashSet<Sessions>();
            CreatedAt = DateTime.Now;
            Role = "Student";
            EmailNotificationsEnabled = true;
            DueDateRemindersEnabled = true;
            OverdueNotificationsEnabled = true;
            ReminderDaysBeforeDue = 2;
            PreferredEmailFormat = "HTML";
        }
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression("Admin|Student")]
        public string Role { get; set; } = "Student";
[Required]
public DateTime CreatedAt { get; set; } = DateTime.Now;

// Notification Preferences
public bool EmailNotificationsEnabled { get; set; } = true;
public bool DueDateRemindersEnabled { get; set; } = true;
public bool OverdueNotificationsEnabled { get; set; } = true;
public int ReminderDaysBeforeDue { get; set; } = 2;

// Email Settings
[StringLength(100)]
public string PreferredEmailFormat { get; set; } = "HTML"; // HTML or Plain Text

// Notification Tracking
public DateTime? LastNotificationSent { get; set; }
public DateTime? LastEmailVerifiedAt { get; set; }

// Navigation Properties
public virtual ICollection<BorrowRecords> BorrowRecords { get; set; }
public virtual ICollection<Sessions> Sessions { get; set; }

    }
}
