using Rafeeq.DTOs.Articles;

namespace Rafeeq.Services.Articles
{
    public interface IArticleService
    {
        Task<IEnumerable<ArticleDto>> GetPublishedArticlesAsync(string? category = null);
        Task<ArticleDto?> GetArticleByIdAsync(int id);
        Task IncrementArticleViewCountAsync(int id);
    }
}
