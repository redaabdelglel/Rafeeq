using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.Models
{
    public class AIConfiguration
    {
        [Key]
        public int ConfigId { get; set; }

        [Required]
        [StringLength(100)]
        public string ConfigKey { get; set; } = string.Empty;

        [Required]
        public string ConfigValue { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ConfigType { get; set; } 

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
