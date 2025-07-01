using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.FAQ
{
    public class FAQRepository : RepositoryBase<Rafeeq.Models.FAQ>, IFAQRepository
    {
        public FAQRepository(RafeeqContext context) : base(context) { }

        public IQueryable<Rafeeq.Models.FAQ> GetActiveFAQQuery(string? category = null, string? searchQuery = null)
        {
            var query = GetQuery().Where(f => f.IsActive == true);
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(f => f.Category == category);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var lowerCaseQuery = searchQuery.ToLower();
                query = query.Where(f =>
                    f.Question.ToLower().Contains(lowerCaseQuery) ||
                    f.Answer.ToLower().Contains(lowerCaseQuery)
                );
            }

            return query.OrderBy(f => f.SortOrder);
        }

        public IQueryable<string> GetFAQCategoriesQuery()
        {
            return GetQuery()
                   .Where(f => f.IsActive == true && f.Category != null)
                   .Select(f => f.Category!)
                   .Distinct();
        }

        public async Task<Rafeeq.Models.FAQ?> GetFaqByIdAsync(int faqId)
        {
            return await Context.Set<Rafeeq.Models.FAQ>()
                                .FirstOrDefaultAsync(f => f.FAQId == faqId);
        }
    }
}
