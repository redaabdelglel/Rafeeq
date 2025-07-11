
using Rafeeq.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Forum
{
    public interface IForumCategoryRepository
    {
        Task<List<ForumCategory>> GetAllWithPostCountAsync();
        Task<ForumCategory?> GetByIdAsync(int id);
        Task AddAsync(ForumCategory category);
        void Update(ForumCategory category);
        void Delete(ForumCategory category);
        Task SaveAsync();
    }
}
