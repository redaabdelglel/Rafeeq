namespace Rafeeq.DTOs.Articles
{
    public class ArticleListDto
    {
        public int ArticleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? Category { get; set; }
        public string? AuthorName { get; set; }
        public bool IsPublished { get; set; }   
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
