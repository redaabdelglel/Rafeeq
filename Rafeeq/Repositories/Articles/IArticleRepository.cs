using Microsoft.EntityFrameworkCore.Query;
using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.Articles
{
    public interface IArticleRepository: IRepositoryBase<Article>
    {
        IQueryable<Article> GetPublishedArticlesQuery(string? category = null, string? searchQuery = null);

        Task<Article?> GetByIdWithAuthorAsync(int id);
    }
}
