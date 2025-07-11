using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.Models
{
    public class TTSCache
    {
        [Key]
        public int CacheId { get; set; }

        [Required]
        [StringLength(64)]
        public string TextHash { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string AudioFilePath { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Voice { get; set; } = string.Empty;

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime")]
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
    }
}
