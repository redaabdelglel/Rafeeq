using Rafeeq.Models;

namespace Rafeeq.Repositories.AI
{
    public interface ITTSCacheRepository
    {
        Task<TTSCache?> GetByTextHashAsync(string textHash, string voice);
        Task AddAsync(TTSCache cache);
        Task UpdateLastUsedAsync(int cacheId);
    }
}
