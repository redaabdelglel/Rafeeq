using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rafeeq.DTOs.AI;
using Rafeeq.Services.AI;



namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmbeddingController : ControllerBase
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger<EmbeddingController> _logger;

        public EmbeddingController(IEmbeddingService embeddingService, ILogger<EmbeddingController> logger)
        {
            _embeddingService = embeddingService;
            _logger = logger;
        }

        [HttpPost("generate-mentor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateMentorEmbedding([FromBody] EmbeddingGenerationRequest request)
        {
            try
            {
                _logger.LogInformation("🚀 Starting mentor embedding generation for ID: {MentorId}", request?.MentorId);

                if (request == null)
                {
                    _logger.LogWarning("❌ Request is null");
                    return BadRequest(new { success = false, message = "Request body is null" });
                }

                if (request.MentorId <= 0)
                {
                    _logger.LogWarning("❌ Invalid mentor ID: {MentorId}", request.MentorId);
                    return BadRequest(new { success = false, message = "Invalid mentor ID" });
                }

                _logger.LogInformation("✅ Request validation passed for mentor ID: {MentorId}", request.MentorId);

                _logger.LogInformation("🔄 Calling embedding service...");
                var result = await _embeddingService.GenerateMentorEmbeddingAsync(request.MentorId);

                _logger.LogInformation("📊 Service response: Success={Success}, Message={Message}",
                    result.Success, result.Message);

                if (result.Success)
                {
                    _logger.LogInformation("✅ Embedding generation successful for mentor {MentorId}", request.MentorId);
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        mentorId = request.MentorId,
                        lastUpdated = result.LastUpdated
                    });
                }

                _logger.LogWarning("❌ Embedding generation failed: {Message}", result.Message);
                return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Exception in GenerateMentorEmbedding: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = ex.Message,
                    stackTrace = ex.StackTrace?.Split('\n').Take(3)
                });
            }
        }

        [HttpPost("mentors/semantic-search")]
        
        public async Task<IActionResult> SemanticMentorSearch([FromBody] SemanticSearchRequest request)
        {
            try
            {
                _logger.LogInformation("🔍 Starting semantic search with query: {Query}", request?.Query);

                if (request == null)
                {
                    _logger.LogWarning("❌ Search request is null");
                    return BadRequest(new { success = false, message = "Request body is null" });
                }

                if (string.IsNullOrEmpty(request.Query))
                {
                    _logger.LogWarning("❌ Search query is empty");
                    return BadRequest(new { success = false, message = "Search query is required" });
                }

                if (request.Query.Length > 1000)
                {
                    _logger.LogWarning("❌ Search query too long: {Length} characters", request.Query.Length);
                    return BadRequest(new { success = false, message = "Search query too long (max 1000 characters)" });
                }

                _logger.LogInformation("✅ Search request validation passed. Query length: {Length}", request.Query.Length);

                _logger.LogInformation("🔍 Search parameters: MaxRate={MaxRate}, MinRating={MinRating}, Skills={Skills}",
                    request.MaxHourlyRate, request.MinRating, string.Join(",", request.Skills ?? new List<string>()));

                _logger.LogInformation("🔄 Calling semantic search service...");
                var result = await _embeddingService.SemanticMentorSearchAsync(request);

                _logger.LogInformation("📊 Search response: Success={Success}, Results={Count}, Time={Time}ms",
                    result.Success, result.TotalResults, result.SearchTime);

                if (result.Success)
                {
                    _logger.LogInformation("✅ Semantic search successful. Found {Count} mentors in {Time}ms",
                        result.TotalResults, result.SearchTime);
                    return Ok(new
                    {
                        success = true,
                        mentors = result.Mentors,
                        totalResults = result.TotalResults,
                        searchTime = result.SearchTime
                    });
                }

                _logger.LogWarning("❌ Semantic search failed: {Message}", result.Message);
                return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Exception in SemanticMentorSearch: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Search error",
                    error = ex.Message
                });
            }
        }


        [HttpPost("bulk-generate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkGenerateEmbeddings()
        {
            try
            {
                var result = await _embeddingService.BulkGenerateEmbeddingsAsync();

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    processedMentors = result.ProcessedCount,
                    errors = result.Errors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("mentor/{mentorId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMentorEmbedding(int mentorId)
        {
            try
            {
                var result = await _embeddingService.DeleteMentorEmbeddingAsync(mentorId);

                if (result)
                {
                    return Ok(new { success = true, message = "Embedding deleted successfully" });
                }

                return NotFound(new { success = false, message = "Embedding not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
