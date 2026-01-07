using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ShareItems_WebApp.Entities
{
    [Index(nameof(Code), IsUnique = true)]
    public class Note
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(50000)]
        public string? Content { get; set; }

        [MaxLength(500)]
        public string? Pin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<NoteFile> Files { get; set; } = new List<NoteFile>();
    }
}
