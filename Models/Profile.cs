using System.ComponentModel.DataAnnotations;

namespace grupp6WebApp.Models;

public class Profile
{
    public int Id { get; set; }

    [Required]
    public string IdentityUserId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }
}
