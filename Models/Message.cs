using System.ComponentModel.DataAnnotations;

namespace group6WebApp.Models;

public class Message
{
    [Key]
    public int MessageId { get; set; }

    public string? SenderName { get; set; }

    public int ReceiverUserId { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;
    public DateTime SentDate { get; set; } = DateTime.Now;

    public User? ReceiverUser { get; set; }
}
