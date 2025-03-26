using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Business.Models
{
    public class NotificationLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int NotificationTemplateId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Type { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public bool Sent { get; set; } = false;

        public DateTime? SentAt { get; set; }

        [MaxLength(500)]
        public string ErrorMessage { get; set; }

        public int? BorrowRecordId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual NotificationTemplate NotificationTemplate { get; set; }
        public virtual BorrowRecords BorrowRecord { get; set; }
    }
}