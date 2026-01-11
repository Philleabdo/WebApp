using System.ComponentModel.DataAnnotations;
using grupp6WebApp.Models;

namespace grupp6WebApp.Models;

public class Message
{
    [Key]
    public int MessageId { get; set; }
    [Required]
    [StringLength(50)]
    public string? SenderName { get; set; }
    [Required]
    public int ReceiverUserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;
    public DateTime SentDate { get; set; } = DateTime.Now;

    // Navigation property
    public User? ReceiverUser { get; set; }
}
