using System.ComponentModel.DataAnnotations;

namespace grupp6WebApp.Models;

public class Project
{
    [Key]
    public int ProjectId { get; set; }

    public int UserId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? ProjectUrl { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public User? User { get; set; }
}
