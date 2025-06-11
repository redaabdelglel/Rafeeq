using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.CV;
using Rafeeq.Services.CV;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CVsController : ControllerBase
    {
        private readonly CVService _cvService;
        private readonly ILogger<CVsController> _logger;

        public CVsController(CVService cvService, ILogger<CVsController> logger)
        {
            _cvService = cvService;
            _logger = logger;
        }

        // POST: api/cvs/comments
        [HttpPost("comments")]
        [Authorize]
        public async Task<IActionResult> AddComment([FromBody] AddCVCommentDto dto)
        {
            try
            {
                // Get current user ID from claims
                var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (mentorId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _cvService.AddCommentAsync(dto, mentorId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding CV comment");
                return StatusCode(500, new { success = false, message = "An error occurred while adding the comment", error = ex.Message });
            }
        }

        // DELETE: api/cvs/comments/{id}
        [HttpDelete("comments/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                // Get current user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _cvService.DeleteCommentAsync(id, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting CV comment {id}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the comment", error = ex.Message });
            }
        }
    }
}
