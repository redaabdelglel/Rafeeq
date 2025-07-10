using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.Models
{
    public class ForumPostReport
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int ReportedByUserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Reason { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // e.g. Pending, Resolved, Ignored

        [StringLength(255)]
        public string? AdminNote { get; set; }

        [ForeignKey(nameof(PostId))]
        public virtual ForumPost Post { get; set; }

        [ForeignKey(nameof(ReportedByUserId))]
        public virtual User ReportedByUser { get; set; }
    }
}
