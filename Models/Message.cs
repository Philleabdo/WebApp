using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grupp6WebApp.Models
{
    [Table("Message")]
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        public string? SenderName { get; set; } // Namn för t.ex. anonyma avsändare

        [Required]
        public int ReceiverUserId { get; set; } // Mottagarens ID

        [Required]
        [StringLength(255)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false; // Markera som läst/oläst

        public DateTime SentDate { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("ReceiverUserId")]
        public virtual User? Receiver { get; set; }
    }
}