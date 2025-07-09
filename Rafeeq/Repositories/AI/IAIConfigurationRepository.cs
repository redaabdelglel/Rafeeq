using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.AI
{
    public interface IAIConfigurationRepository : IRepositoryBase<AIConfiguration>
    {
        Task<AIConfiguration?> GetByKeyAsync(string key);
        Task<IEnumerable<AIConfiguration>> GetByTypeAsync(string type);
        Task<bool> UpdateConfigValueAsync(string key, string value);
    }
}
