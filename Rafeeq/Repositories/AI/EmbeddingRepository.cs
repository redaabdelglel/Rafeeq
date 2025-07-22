using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.AI
{
    public class EmbeddingRepository : RepositoryBase<MentorEmbedding>, IEmbeddingRepository
    {
        public EmbeddingRepository(RafeeqContext context) : base(context)
        {
        }

        public async Task<MentorEmbedding?> GetByUserIdAsync(int userId)
        {
            return await Context.MentorEmbeddings.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        public async Task<IEnumerable<MentorEmbedding>> GetAllMentorEmbeddingsAsync()
        {
            return await Context.MentorEmbeddings.Include(e => e.User).ToListAsync();
        }

        public async Task<bool> DeleteByUserIdAsync(int userId)
        {
            var embedding = await GetByUserIdAsync(userId);
            if (embedding != null)
            {
                Context.MentorEmbeddings.Remove(embedding);
                return true;
            }
            return false;
        }

        public async Task<int> GetTotalEmbeddingsCountAsync()
        {
            return await Context.MentorEmbeddings.CountAsync();
        }

        public async Task<IEnumerable<MentorEmbedding>> GetEmbeddingsWithSimilarityAsync(byte[] queryEmbedding, float threshold = 0.7f)
        {
           
            return await Context.MentorEmbeddings.Include(e => e.User)
                .ThenInclude(u => u.MentorSkills)
                .ThenInclude(ms => ms.Skill)
                .ToListAsync();
        }
    }
}
