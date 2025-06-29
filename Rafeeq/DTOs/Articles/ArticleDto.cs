namespace Rafeeq.DTOs.Articles
{
    public class ArticleDto
    {
        public int ArticleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Category { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty; 
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
