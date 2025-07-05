using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Articles
{
    public class ArticleCreateDto
    {
        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Summary { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public int? AuthorId { get; set; }

        public bool IsPublished { get; set; } = false;
    }
}
