using System.ComponentModel.DataAnnotations;

namespace grupp6WebApp.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true; // Standardvärde 1 (Aktivt konto)

    // Här lagras HASH (BCrypt)
    [Required]
    public string Password { get; set; } = string.Empty;

    // Navigation
    public Profile? Profile { get; set; }
    public List<Project> Projects { get; set; } = new();
    public List<Message> ReceivedMessages { get; set; } = new();
}
