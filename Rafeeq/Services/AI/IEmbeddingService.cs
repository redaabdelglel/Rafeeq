using Rafeeq.DTOs.AI;

namespace Rafeeq.Services.AI
{
    public interface IEmbeddingService
    {
        Task<EmbeddingGenerationResponse> GenerateMentorEmbeddingAsync(int mentorId);
        Task<SemanticSearchResponse> SemanticMentorSearchAsync(SemanticSearchRequest request);
        Task<BulkGenerationResponse> BulkGenerateEmbeddingsAsync();
        Task<bool> DeleteMentorEmbeddingAsync(int mentorId);
    }
}

