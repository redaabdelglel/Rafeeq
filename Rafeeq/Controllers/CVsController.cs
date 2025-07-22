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
using Rafeeq.UnitOfWork;

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
        private readonly UnitOfWorkManager _unitOfWork;

        public CVsController(CVService cvService, ILogger<CVsController> logger, RafeeqContext context,
                            IMapper mapper, UnitOfWorkManager unitOfWork)
        {
            _cvService = cvService;
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        // POST: api/cvs/comments
        [HttpPost("comments")]
        [Authorize]
        public async Task<IActionResult> AddComment([FromBody] AddCVCommentDto dto)
        {
            try
            {
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
                var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (mentorId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var mentor = await _context.Users.FirstOrDefaultAsync(u => u.UserId == mentorId);
                if (mentor == null || !mentor.IsMentor.GetValueOrDefault())
                {
                    return Forbid();
                }

                var cvs = await _context.MenteeCVs
                    .Include(cv => cv.User)
                    .Where(cv => cv.IsActive && _context.Bookings.Any(b =>
                        b.MentorId == mentorId &&
                        b.MenteeId == cv.UserId &&
                        (b.Status == "Confirmed" || b.Status == "Completed")))
                    .OrderByDescending(cv => cv.UploadDate)
                    .ToListAsync();

                var result = cvs.Select(cv => new MenteeCVDto
                {
                    CVId = cv.CVId,
                    UserId = cv.UserId,
                    FileName = cv.FileName,
                    UploadDate = cv.UploadDate,
                    IsActive = cv.IsActive,
                    UserFullName = cv.User?.FullName ?? "Unknown", // Handle null with default value
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

        // GET: api/cvs/download/{fileName}
        [HttpGet("download/{fileName}")]
        [Authorize]
        public async Task<IActionResult> DownloadCV(string fileName)
        {
            try
            {
                var mentorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (mentorId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var mentor = await _unitOfWork.UserRepository.GetByIdAsync(mentorId);
                if (mentor == null || !mentor.IsMentor.GetValueOrDefault())
                {
                    return Forbid();
                }

                var uploadsFolder = Path.Combine("uploads", "cvs");
                var directoryInfo = new DirectoryInfo(uploadsFolder);

                if (!Directory.Exists(uploadsFolder))
                {
                    return NotFound(new { success = false, message = "CV uploads directory not found" });
                }

                var fileInfo = directoryInfo.GetFiles()
                    .FirstOrDefault(f => f.Name.EndsWith(fileName) ||
                                        f.Name == fileName);

                if (fileInfo == null)
                {
                    return NotFound(new { success = false, message = "File not found" });
                }

                var filePath = fileInfo.FullName;

                
                var cv = await _context.MenteeCVs
                    .FirstOrDefaultAsync(cv => cv.FilePath.EndsWith(fileInfo.Name));

                if (cv == null)
                {
                    return NotFound(new { success = false, message = "CV record not found" });
                }

               
                bool hasPermission = await _context.Bookings.AnyAsync(b =>
                    b.MentorId == mentorId &&
                    b.MenteeId == cv.UserId &&
                    (b.Status == "Confirmed" || b.Status == "Completed"));

                if (!hasPermission)
                {
                    return Forbid();
                }

               
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

             
                var contentType = cv.ContentType ?? GetContentTypeFromFileName(fileInfo.Name);

               
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading CV file {fileName}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the file", error = ex.Message });
            }
        }

        private string GetContentTypeFromFileName(string fileName)
        {
           
            if (string.IsNullOrEmpty(fileName))
                return "application/pdf";

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                _ => "application/octet-stream" 
            };
        }
    }
}

