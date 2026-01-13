using System.ComponentModel.DataAnnotations;

namespace grupp6WebApp.Models;

public class Profile
{
    [Key]
    public int ProfileId { get; set; }

    public int UserId { get; set; }

    public string? Bio { get; set; }
    public string? Skills { get; set; }
    public string? Education { get; set; }
    public string? Experience { get; set; }
    public bool IsPublic { get; set; } = true;
    public string? ProfilePictureUrl { get; set; }

    // De nya fälten som din vän har lagt till i databasen:
    public int ViewCount { get; set; } = 0;
    public string? Category { get; set; }

    public User? User { get; set; }
}