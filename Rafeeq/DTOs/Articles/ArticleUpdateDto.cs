using System.ComponentModel.DataAnnotations;

namespace Rafeeq.DTOs.Articles
{
    public class ArticleUpdateDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(300, ErrorMessage = "Title cannot exceed 300 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters.")]
        public string? Summary { get; set; }

        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Author ID is required.")]
        public int AuthorId { get; set; }

        public bool IsPublished { get; set; }
    }
}
