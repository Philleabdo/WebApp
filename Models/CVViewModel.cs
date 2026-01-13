using System.ComponentModel.DataAnnotations;

namespace grupp6WebApp.Models
{
    public class CVViewModel
    {
        public int UserId { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }

        // CV-detaljer från Profile-tabellen
        public string? Bio { get; set; }
        public string? Skills { get; set; }
        public string? Education { get; set; }
        public string? Experience { get; set; }

        [Display(Name = "Make profile public")]
        public bool IsPublic { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public int ViewCount { get; set; }

        // Lista för att visa projekt-titlar
        public List<string> Projects { get; set; } = new List<string>();
    }
}