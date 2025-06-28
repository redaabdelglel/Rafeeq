using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.FAQ
{
    public class FAQRepository : RepositoryBase<Rafeeq.Models.FAQ>, IFAQRepository
    {
        public FAQRepository(RafeeqContext context) : base(context) { }

        public IQueryable<Rafeeq.Models.FAQ> GetActiveFAQQuery(string? category = null)
        {
            var query = GetQuery().Where(f => f.IsActive == true);
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(f => f.Category == category);
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
    }
}
