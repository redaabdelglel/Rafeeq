using Rafeeq.DTOs;
using Rafeeq.DTOs.Articles;

namespace Rafeeq.Services.Articles
{
    public interface IArticleService
    {
        Task<PagedResult<ArticleListDto>> GetPublishedArticlesAsync(
               string? category = null,
               string? searchQuery = null,
               int pageNumber = 1,
               int pageSize = 6);

        Task<ArticleDto?> GetArticleByIdAsync(int id);
        Task IncrementArticleViewCountAsync(int id);
    }
}
