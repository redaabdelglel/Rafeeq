using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;

namespace Rafeeq.Repositories.AI
{
    public class TTSCacheRepository : ITTSCacheRepository
    {
        private readonly RafeeqContext _context;
        public TTSCacheRepository(RafeeqContext context)
        {
            _context = context;
        }

        public async Task<TTSCache?> GetByTextHashAsync(string textHash, string voice)
        {
            return await _context.TTSCaches
                .FirstOrDefaultAsync(c => c.TextHash == textHash && c.Voice == voice);
        }

        public async Task AddAsync(TTSCache cache)
        {
            _context.TTSCaches.Add(cache);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLastUsedAsync(int cacheId)
        {
            var cache = await _context.TTSCaches.FindAsync(cacheId);
            if (cache != null)
            {
                cache.LastUsed = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
