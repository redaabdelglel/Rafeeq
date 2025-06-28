using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.Models
{
    public class Article
    {
        [Key]
        public int ArticleId { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Content { get; set; }

        [StringLength(500)]
        public string? Summary { get; set; }

        [StringLength(100)]
        public string? Category { get; set; } // 'Mentoring', 'Career', 'Interview', 'CV'

        public int? AuthorId { get; set; }

        public bool IsPublished { get; set; } = true;

        public int ViewCount { get; set; } = 0;

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("AuthorId")]
        [InverseProperty("Articles")]
        public virtual User? Author { get; set; }
    }
}
