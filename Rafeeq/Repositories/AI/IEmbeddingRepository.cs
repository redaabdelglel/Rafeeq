using Rafeeq.Models;
using Rafeeq.Repositories.RepositoryBase;

namespace Rafeeq.Repositories.AI
{
    public interface IEmbeddingRepository : IRepositoryBase<MentorEmbedding>
    {
        Task<MentorEmbedding?> GetByUserIdAsync(int userId);
        Task<IEnumerable<MentorEmbedding>> GetAllMentorEmbeddingsAsync();
        Task<bool> DeleteByUserIdAsync(int userId);
        Task<int> GetTotalEmbeddingsCountAsync();
        Task<IEnumerable<MentorEmbedding>> GetEmbeddingsWithSimilarityAsync(byte[] queryEmbedding, float threshold = 0.7f);
    }
}
