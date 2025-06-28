using Rafeeq.Repositories.RepositoryBase;
using System.Linq;
namespace Rafeeq.Repositories.FAQ
{
    public interface IFAQRepository : IRepositoryBase<Rafeeq.Models.FAQ>
    {
        IQueryable<Rafeeq.Models.FAQ> GetActiveFAQQuery(string? category = null); 
        IQueryable<string> GetFAQCategoriesQuery();
    }
}
