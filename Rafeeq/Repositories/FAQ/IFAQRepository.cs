using Rafeeq.Repositories.RepositoryBase;
using System.Linq;
namespace Rafeeq.Repositories.FAQ
{
    public interface IFAQRepository : IRepositoryBase<Rafeeq.Models.FAQ>
    {
        IQueryable<Rafeeq.Models.FAQ> GetActiveFAQQuery(string? category = null, string? searchQuery = null);
        IQueryable<string> GetFAQCategoriesQuery();

        Task<Rafeeq.Models.FAQ?> GetFaqByIdAsync(int faqId);

    }
}
