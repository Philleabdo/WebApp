using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grupp6WebApp.Models
{
    [Table("Project")]
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        public int UserId { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string? ProjectUrl { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
