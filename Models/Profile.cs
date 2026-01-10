using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grupp6WebApp.Models
{
    [Table("Profile")]
    public class Profile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public int UserId { get; set; }

        public string? Bio { get; set; }

        public string? Skills { get; set; } // Kompetenser

        public string? Education { get; set; } // Utbildning

        public string? Experience { get; set; } // Erfarenhet

        public bool IsPublic { get; set; } = true; // Privat/Offentlig

        public string? ProfilePictureUrl { get; set; }

        // Navigation property (Valfritt, men bra för att enkelt hämta tillhörande User)
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
