using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareItems_WebApp.Entities
{
    public class NoteFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NoteId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FileType { get; set; } = string.Empty; // "document", "image", "video", "others"

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        /// <summary>
        /// Cloudinary secure URL for accessing the file
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>
        /// Cloudinary public ID for managing the file (delete, transform, etc.)
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string PublicId { get; set; } = string.Empty;

        /// <summary>
        /// Legacy field - kept for backward compatibility (can be removed in future migration)
        /// </summary>
        [MaxLength(500)]
        public string? FilePath { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(NoteId))]
        public Note Note { get; set; } = null!;
    }
}

