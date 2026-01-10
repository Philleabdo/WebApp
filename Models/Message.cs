using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grupp6WebApp.Models
{
    [Table("Message")]
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        public string? SenderName { get; set; }

        [Required]
        public int ReceiverUserId { get; set; } // Kopplas mot din User.UserId (int)

        [Required]
        [StringLength(255)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime SentDate { get; set; } = DateTime.Now;

        // Navigation property till din EGEN User-klass
        [ForeignKey("ReceiverUserId")]
        public virtual User? Receiver { get; set; }
    }
}
