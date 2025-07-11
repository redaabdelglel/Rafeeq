using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rafeeq.DTOs.AI;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using System.Text;
using System.Text.Json;

namespace Rafeeq.Services.AI
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<EmbeddingService> _logger;
        private readonly HttpClient _httpClient;

        public EmbeddingService(
            UnitOfWorkManager unitOfWork,
            IMapper mapper,
            ILogger<EmbeddingService> logger,
            HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<EmbeddingGenerationResponse> GenerateMentorEmbeddingAsync(int mentorId)
        {
            try
            {
                // Get mentor data
                var mentor = await _unitOfWork.UserRepository.GetQuery()
                    .Include(u => u.MentorSkills)
                    .ThenInclude(ms => ms.Skill)
                    .FirstOrDefaultAsync(u => u.UserId == mentorId && u.IsMentor == true);

                if (mentor == null)
                {
                    return new EmbeddingGenerationResponse
                    {
                        Success = false,
                        Message = "Mentor not found"
                    };
                }

                // Create text to embed
                var textToEmbed = CreateMentorEmbeddingText(mentor);

                // Generate embedding using OpenAI
                var embedding = await GenerateEmbeddingAsync(textToEmbed);
                if (embedding == null)
                {
                    return new EmbeddingGenerationResponse
                    {
                        Success = false,
                        Message = "Failed to generate embedding"
                    };
                }

                // Save or update embedding
                var existingEmbedding = await _unitOfWork.EmbeddingRepository.GetByUserIdAsync(mentorId);
                if (existingEmbedding != null)
                {
                    existingEmbedding.BioEmbedding = embedding;
                    existingEmbedding.LastUpdated = DateTime.UtcNow;
                    _unitOfWork.EmbeddingRepository.Update(existingEmbedding);
                }
                else
                {
                    var newEmbedding = new MentorEmbedding
                    {
                        UserId = mentorId,
                        BioEmbedding = embedding,
                        LastUpdated = DateTime.UtcNow
                    };
                    _unitOfWork.EmbeddingRepository.Add(newEmbedding);
                }

                await _unitOfWork.SaveAsync();

                return new EmbeddingGenerationResponse
                {
                    Success = true,
                    Message = "Embedding generated successfully",
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding for mentor {MentorId}", mentorId);
                return new EmbeddingGenerationResponse
                {
                    Success = false,
                    Message = $"Error generating embedding: {ex.Message}"
                };
            }
        }

        public async Task<SemanticSearchResponse> SemanticMentorSearchAsync(SemanticSearchRequest request)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Generate embedding for search query
                var queryEmbedding = await GenerateEmbeddingAsync(request.Query);
                if (queryEmbedding == null)
                {
                    return new SemanticSearchResponse
                    {
                        Success = false,
                        Message = "Failed to process search query"
                    };
                }

                // Get all mentor embeddings
                var mentorEmbeddings = await _unitOfWork.EmbeddingRepository.GetEmbeddingsWithSimilarityAsync(queryEmbedding);

                // Calculate similarities and filter
                var results = new List<MentorResult>();
                var similarityThreshold = await GetSimilarityThreshold();

                foreach (var embedding in mentorEmbeddings)
                {
                    if (embedding.BioEmbedding != null && embedding.User != null)
                    {
                        var similarity = CalculateCosineSimilarity(queryEmbedding, embedding.BioEmbedding);

                        if (similarity >= similarityThreshold)
                        {
                            var mentorResult = CreateMentorResult(embedding.User, similarity);

                            // Apply additional filters
                            if (PassesFilters(mentorResult, request))
                            {
                                results.Add(mentorResult);
                            }
                        }
                    }
                }

                // Sort by similarity score descending and take only MaxResults
                int maxResults = request.MaxResults > 0 ? request.MaxResults : 3;
                results = results
                    .OrderByDescending(r => r.SimilarityScore)
                    .Take(maxResults)
                    .ToList();

                stopwatch.Stop();

                return new SemanticSearchResponse
                {
                    Success = true,
                    Mentors = results,
                    TotalResults = results.Count,
                    SearchTime = (float)stopwatch.ElapsedMilliseconds,
                    Message = "Search completed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in semantic search");
                return new SemanticSearchResponse
                {
                    Success = false,
                    Message = $"Search error: {ex.Message}",
                    SearchTime = (float)stopwatch.ElapsedMilliseconds
                };
            }
        }


        public async Task<BulkGenerationResponse> BulkGenerateEmbeddingsAsync()
        {
            try
            {
                var mentors = await _unitOfWork.UserRepository.GetQuery()
                    .Include(u => u.MentorSkills)
                    .ThenInclude(ms => ms.Skill)
                    .Where(u => u.IsMentor == true && u.IsActive == true)
                    .ToListAsync();

                var processedCount = 0;
                var errors = new List<string>();

                foreach (var mentor in mentors)
                {
                    try
                    {
                        var result = await GenerateMentorEmbeddingAsync(mentor.UserId);
                        if (result.Success)
                        {
                            processedCount++;
                            _logger.LogInformation("Generated embedding for mentor {MentorId}", mentor.UserId);
                        }
                        else
                        {
                            errors.Add($"Mentor {mentor.UserId}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Mentor {mentor.UserId}: {ex.Message}");
                        _logger.LogError(ex, "Error generating embedding for mentor {MentorId}", mentor.UserId);
                    }

                    // Add small delay to avoid rate limiting
                    await Task.Delay(100);
                }

                return new BulkGenerationResponse
                {
                    Success = true,
                    Message = $"Processed {processedCount} mentors",
                    ProcessedCount = processedCount,
                    Errors = errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk generation");
                return new BulkGenerationResponse
                {
                    Success = false,
                    Message = $"Bulk generation failed: {ex.Message}",
                    ProcessedCount = 0,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<bool> DeleteMentorEmbeddingAsync(int mentorId)
        {
            try
            {
                var deleted = await _unitOfWork.EmbeddingRepository.DeleteByUserIdAsync(mentorId);
                if (deleted)
                {
                    await _unitOfWork.SaveAsync();
                    _logger.LogInformation("Deleted embedding for mentor {MentorId}", mentorId);
                }
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting embedding for mentor {MentorId}", mentorId);
                return false;
            }
        }

        // Helper methods
        private async Task<byte[]?> GenerateEmbeddingAsync(string text)
        {
            try
            {
                _logger.LogInformation("Starting embedding generation for text length: {Length}", text.Length);
                _logger.LogInformation("Text preview: {TextPreview}", text.Substring(0, Math.Min(200, text.Length)));

                var apiKey = await GetOpenAIApiKey();
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("OpenAI API key not found or empty");
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    input = text,
                    model = "text-embedding-3-small"
                };

                var jsonBody = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("OpenAI API response status: {StatusCode}, content: {Content}", response.StatusCode, responseContent);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var embeddingResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                        if (!embeddingResponse.TryGetProperty("data", out var data) || data.GetArrayLength() == 0)
                        {
                            _logger.LogError("OpenAI API response missing 'data' or empty: {Content}", responseContent);
                            return null;
                        }

                        var embeddingArray = data[0].GetProperty("embedding")
                            .EnumerateArray()
                            .Select(x => x.GetSingle())
                            .ToArray();

                        return FloatArrayToByteArray(embeddingArray);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse OpenAI embedding response: {Content}", responseContent);
                        return null;
                    }
                }
                else
                {
                    _logger.LogError("OpenAI API error: {StatusCode} - {Error}", response.StatusCode, responseContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detailed error in GenerateEmbeddingAsync: {Message}", ex.Message);
                return null;
            }
        }


        private string CreateMentorEmbeddingText(User mentor)
        {
            var sb = new StringBuilder();

            try
            {
                // Add mentor name for context
                sb.AppendLine($"Mentor: {mentor.FullName ?? "Unknown"}");

                // Add mentor bio
                if (!string.IsNullOrEmpty(mentor.Bio))
                {
                    sb.AppendLine(mentor.Bio);
                }
                else
                {
                    sb.AppendLine("No bio available.");
                }

                // Add skills
                if (mentor.MentorSkills?.Any() == true)
                {
                    var skills = mentor.MentorSkills
                        .Where(ms => ms.Skill != null && !string.IsNullOrEmpty(ms.Skill.Name))
                        .Select(ms => ms.Skill!.Name)
                        .ToList();

                    if (skills.Any())
                    {
                        sb.AppendLine("Skills: " + string.Join(", ", skills));
                    }
                    else
                    {
                        sb.AppendLine("Skills: General mentoring");
                    }
                }
                else
                {
                    sb.AppendLine("Skills: General mentoring");
                }

                // Add role information
                var roles = new List<string>();
                if (mentor.IsMentor == true) roles.Add("Mentor");
                if (mentor.IsInterviewer == true) roles.Add("Interviewer");

                if (roles.Any())
                {
                    sb.AppendLine($"Role: {string.Join(", ", roles)}");
                }
                else
                {
                    sb.AppendLine("Role: Mentor");
                }

                // Add hourly rate for context
                if (mentor.HourlyRate.HasValue && mentor.HourlyRate > 0)
                {
                    sb.AppendLine($"Hourly Rate: ${mentor.HourlyRate.Value}");
                }

                var result = sb.ToString();
                _logger.LogInformation("Created mentor text for {MentorId}: {Length} characters", mentor.UserId, result.Length);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating mentor embedding text for {MentorId}", mentor.UserId);
                return $"Mentor: {mentor.FullName ?? "Unknown"}\nGeneral mentoring services available.";
            }
        }


        private async Task<string?> GetOpenAIApiKey()
        {
            var config = await _unitOfWork.AIConfigurationRepository.GetByKeyAsync("openai_api_key");
            return config?.ConfigValue;
        }

        private async Task<float> GetSimilarityThreshold()
        {
            var config = await _unitOfWork.AIConfigurationRepository.GetByKeyAsync("similarity_threshold");
            if (config != null && float.TryParse(config.ConfigValue, out var threshold))
            {
                return threshold;
            }
            return 0.7f; // Default threshold
        }

        private float CalculateCosineSimilarity(byte[] vector1, byte[] vector2)
        {
            if (vector1 == null || vector2 == null)
                return 0f;

            var array1 = ByteArrayToFloatArray(vector1);
            var array2 = ByteArrayToFloatArray(vector2);

            if (array1.Length != array2.Length)
                return 0f;

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < array1.Length; i++)
            {
                dotProduct += array1[i] * array2[i];
                magnitude1 += array1[i] * array1[i];
                magnitude2 += array2[i] * array2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0f;

            return (float)(dotProduct / (magnitude1 * magnitude2));
        }

        private MentorResult CreateMentorResult(User mentor, float similarityScore)
        {
            // Calculate average rating from reviews
            var averageRating = CalculateAverageRating(mentor);

            return new MentorResult
            {
                UserId = mentor.UserId,
                FullName = mentor.FullName ?? "",
                Bio = mentor.Bio ?? "",
                HourlyRate = mentor.HourlyRate ?? 0,
                SimilarityScore = similarityScore,
                Skills = mentor.MentorSkills?
                    .Where(ms => ms.Skill != null && !string.IsNullOrEmpty(ms.Skill.Name))
                    .Select(ms => ms.Skill!.Name)
                    .ToList() ?? new List<string>(),
                AverageRating = averageRating,
                ProfilePicture = mentor.ProfilePicture ?? ""
            };
        }

        private bool PassesFilters(MentorResult mentor, SemanticSearchRequest request)
        {
            // Check hourly rate filter
            if (request.MaxHourlyRate.HasValue && mentor.HourlyRate > request.MaxHourlyRate.Value)
                return false;

            // Check rating filter
            if (request.MinRating.HasValue && mentor.AverageRating < request.MinRating.Value)
                return false;

            // Check skills filter
            if (request.Skills?.Any() == true)
            {
                var hasAnySkill = request.Skills.Any(skill =>
                    mentor.Skills.Any(mentorSkill =>
                        mentorSkill.Contains(skill, StringComparison.OrdinalIgnoreCase)));

                if (!hasAnySkill)
                    return false;
            }

            return true;
        }

        private double CalculateAverageRating(User mentor)
        {
            try
            {
                // ✅ Use UnitOfWork context directly since ReviewRepository doesn't inherit from RepositoryBase
                var reviews = _unitOfWork.context.Reviews
                    .Where(r => r.ReviewedUserId == mentor.UserId)
                    .ToList();

                if (reviews.Any())
                {
                    return reviews.Average(r => r.Rating ?? 0);
                }

                return 4.5; // Default rating for new mentors
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating average rating for mentor {MentorId}", mentor.UserId);
                return 4.5; // Fallback rating
            }
        }


        private byte[] FloatArrayToByteArray(float[] floatArray)
        {
            byte[] byteArray = new byte[floatArray.Length * 4];
            Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        private float[] ByteArrayToFloatArray(byte[] byteArray)
        {
            float[] floatArray = new float[byteArray.Length / 4];
            Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
            return floatArray;
        }

    }
}
