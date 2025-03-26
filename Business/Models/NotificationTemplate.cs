using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business.Models
{
    public class NotificationTemplate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }

        [Required]
        public string Template { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Template Type (e.g., DUE_REMINDER, OVERDUE_NOTICE, etc.)
        [Required]
        [MaxLength(50)]
        public string Type { get; set; }

        // Days before due date to send reminder (for due date reminders)
        public int? DaysBeforeDue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}