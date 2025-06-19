using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Rafeeq.DTOs.CV;
using Rafeeq.Services.CV;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Rafeeq.Models;
using Microsoft.EntityFrameworkCore;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CVsController : ControllerBase
    {
        private readonly CVService _cvService;
        private readonly ILogger<CVsController> _logger;
        private readonly RafeeqContext _context;
        private readonly IMapper _mapper;

        public CVsController(CVService cvService, ILogger<CVsController> logger, RafeeqContext context, IMapper mapper)
        {
            _cvService = cvService;
            _logger = logger;
            _context = context;
            _mapper = mapper;
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

        // GET: api/cvs/for-review
        [HttpGet("for-review")]
        [Authorize]
        public async Task<IActionResult> GetCVsForReview()
        {
            try
            {
                // Get current user ID from claims
                var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (mentorId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                // Check if user is a mentor
                var mentor = await _context.Users.FirstOrDefaultAsync(u => u.UserId == mentorId);
                if (mentor == null || !mentor.IsMentor.GetValueOrDefault())
                {
                    return Forbid();
                }

                // Get CVs from mentees who had sessions with this mentor
                var cvs = await _context.MenteeCVs
                    .Include(cv => cv.User)
                    .Where(cv => cv.IsActive && _context.Bookings.Any(b =>
                        b.MentorId == mentorId &&
                        b.MenteeId == cv.UserId &&
                        (b.Status == "Confirmed" || b.Status == "Completed")))
                    .OrderByDescending(cv => cv.UploadDate)
                    .ToListAsync();

                // Map to DTO with additional mentee information
                var result = cvs.Select(cv => new MenteeCVDto
                {
                    CVId = cv.CVId,
                    UserId = cv.UserId,
                    FileName = cv.FileName,
                    UploadDate = cv.UploadDate,
                    IsActive = cv.IsActive,
                    UserFullName = cv.User?.FullName,
                    // Calculate download URL (relative path that frontend can use)
                    DownloadUrl = $"/uploads/cvs/{System.IO.Path.GetFileName(cv.FilePath)}"
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving CVs for review");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving CVs", error = ex.Message });
            }
        }
    }
}
