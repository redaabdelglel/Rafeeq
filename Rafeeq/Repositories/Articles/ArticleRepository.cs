using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 

namespace Rafeeq.Repositories.Articles
{
    public class ArticleRepository : RepositoryBase<Article>, IArticleRepository
    {
        public ArticleRepository(RafeeqContext context) : base(context) { }

        public IQueryable<Article> GetPublishedArticlesQuery(string? category = null, string? searchQuery = null)
        {
            var query = Context.Set<Article>()
                               .Where(a => a.IsPublished == true);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(a => a.Category == category);
            }

           
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var lowerCaseQuery = searchQuery.ToLower();
                query = query.Where(a =>
                    a.Title.ToLower().Contains(lowerCaseQuery) ||
                    (a.Summary != null && a.Summary.ToLower().Contains(lowerCaseQuery)) ||
                    a.Content.ToLower().Contains(lowerCaseQuery)
                );
            }

            return query.Include(a => a.Author);
        }

        public async Task<Article?> GetByIdWithAuthorAsync(int id)
        {
            return await Context.Set<Article>()
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == id);
        }
    }
}