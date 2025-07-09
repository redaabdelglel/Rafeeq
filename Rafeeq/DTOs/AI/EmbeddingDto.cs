namespace Rafeeq.DTOs.AI
{
    public class SemanticSearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public int? MinRating { get; set; }
        public decimal? MaxHourlyRate { get; set; }
        public List<string> Skills { get; set; } = new();
        public int MaxResults { get; set; } = 3; // Default to 3, or set to 1/2 as you prefer
    }


    public class SemanticSearchResponse
    {
        public List<MentorResult> Mentors { get; set; } = new();
        public float SearchTime { get; set; }
        public int TotalResults { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class MentorResult
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public float SimilarityScore { get; set; } // 0-1
        public List<string> Skills { get; set; } = new();
        public double AverageRating { get; set; }
        public string ProfilePicture { get; set; } = string.Empty;
    }

    public class EmbeddingGenerationRequest
    {
        public int MentorId { get; set; }
    }

    public class EmbeddingGenerationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? LastUpdated { get; set; }
    }

    public class BulkGenerationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ProcessedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
