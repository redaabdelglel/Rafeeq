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


    

        Task<PagedResult<ArticleListDto>> GetAllArticlesForAdminAsync( 
             string? category = null,
             string? searchQuery = null,
             int pageNumber = 1,
             int pageSize = 10);
        Task<ArticleDto?> GetArticleByIdForAdminAsync(int id); 
        Task<ArticleDto> CreateArticleAsync(ArticleCreateDto articleDto);
        Task<ArticleDto?> UpdateArticleAsync(int id, ArticleUpdateDto articleDto);
        Task<bool> DeleteArticleAsync(int id);
    }
}
