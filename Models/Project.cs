using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; 

namespace grupp6WebApp.Models;

public class Project
{
    [Key]
    public int ProjectId { get; set; }

    public int UserId { get; set; } // Skaparen/Ägaren

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? ProjectUrl { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public User? User { get; set; } // Navigering till skaparen

    // NY RAD: Möjliggör Many-to-Many
    public virtual ICollection<User> UsersWhoDisplay { get; set; } = new List<User>();
}
