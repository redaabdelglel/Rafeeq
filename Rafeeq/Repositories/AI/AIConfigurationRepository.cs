using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.AI
{
    public class AIConfigurationRepository : RepositoryBase<AIConfiguration>, IAIConfigurationRepository
    {
        public AIConfigurationRepository(RafeeqContext context) : base(context)
        {
        }

        public async Task<AIConfiguration?> GetByKeyAsync(string key)
        {
            return await Context.AIConfigurations.FirstOrDefaultAsync(c => c.ConfigKey == key && c.IsActive);
        }

        public async Task<IEnumerable<AIConfiguration>> GetByTypeAsync(string type)
        {
            return await Context.AIConfigurations.Where(c => c.ConfigType == type && c.IsActive).ToListAsync();
        }

        public async Task<bool> UpdateConfigValueAsync(string key, string value)
        {
            var config = await GetByKeyAsync(key);
            if (config != null)
            {
                config.ConfigValue = value;
                config.UpdatedAt = DateTime.UtcNow;
                return true;
            }
            return false;
        }
    }
}
