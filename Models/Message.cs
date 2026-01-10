using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace grupp6WebApp.Models
{
    [Table("Message")]
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        public string? SenderName { get; set; } // Namn för t.ex. anonyma avsändare

        // Identity använder string som UserId
        [Required]
        public string ReceiverUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime SentDate { get; set; } = DateTime.Now;

        // Navigation property till IdentityUser
        [ForeignKey(nameof(ReceiverUserId))]
        public IdentityUser? Receiver { get; set; }
    }
}
