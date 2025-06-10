using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.CV;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using AutoMapper;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MenteeCVsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MenteeCVsController> _logger;

        public MenteeCVsController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MenteeCVsController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
        }

        // GET: api/cvs/mentee/{menteeId}
        [HttpGet("mentee/{menteeId}")]
        public async Task<ActionResult<IEnumerable<MenteeCVDto>>> GetMenteeCVs(int menteeId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Verify permissions if user is requesting CVs for someone else
                if (menteeId != currentUserId && currentUserRole != "Admin" && currentUserRole != "Mentor")
                {
                    return Forbid();
                }

                var cvs = await _unitOfWork.CVs.GetMenteeCVsAsync(menteeId);
                return Ok(_mapper.Map<IEnumerable<MenteeCVDto>>(cvs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CVs for mentee {MenteeId}", menteeId);
                return StatusCode(500, "An error occurred while retrieving CVs");
            }
        }

        // POST: api/cvs/mentee/{menteeId}
        [HttpPost("mentee/{menteeId}")]
        public async Task<ActionResult<MenteeCVDto>> UploadCV(int menteeId, [FromForm] UploadCVDTO uploadCVDTO)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (currentUserId != menteeId)
                {
                    return Forbid();
                }

                if (uploadCVDTO.File == null || uploadCVDTO.File.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                var uploadsFolder = Path.Combine("uploads", "cvs");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + uploadCVDTO.File.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                Directory.CreateDirectory(uploadsFolder);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadCVDTO.File.CopyToAsync(fileStream);
                }

                var cv = new MenteeCV
                {
                    UserId = menteeId,
                    FilePath = filePath,
                    FileName = uploadCVDTO.File.FileName,
                    FileSize = (int)uploadCVDTO.File.Length,
                    ContentType = uploadCVDTO.File.ContentType,
                    UploadDate = DateTime.Now,
                    IsActive = true
                };

                var createdCV = await _unitOfWork.CVs.UploadCVAsync(cv);
                return CreatedAtAction(nameof(GetMenteeCVs),
                    new { menteeId = menteeId },
                    _mapper.Map<MenteeCVDto>(createdCV));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading CV for mentee {MenteeId}", menteeId);
                return StatusCode(500, "An error occurred while uploading the CV");
            }
        }

        // DELETE: api/cvs/mentee/{menteeId}/{id}
        [HttpDelete("mentee/{menteeId}/{id}")]
        public async Task<IActionResult> DeleteCV(int menteeId, int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();
                var cv = await _unitOfWork.CVs.GetCVByIdAsync(id);

                if (cv == null)
                {
                    return NotFound();
                }

                // Verify permissions
                if (cv.UserId != menteeId)
                {
                    return BadRequest("CV does not belong to specified mentee");
                }

                if (menteeId != currentUserId && currentUserRole != "Admin")
                {
                    return Forbid();
                }

                var result = await _unitOfWork.CVs.DeleteCVAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting CV {CVId} for mentee {MenteeId}", id, menteeId);
                return StatusCode(500, "An error occurred while deleting the CV");
            }
        }

        // GET: api/cvs/mentee/{menteeId}/comments/{cvId}
        [HttpGet("mentee/{menteeId}/comments/{cvId}")]
        public async Task<ActionResult<IEnumerable<CVCommentDto>>> GetCVComments(int menteeId, int cvId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Verify permissions if user is requesting comments for someone else
                if (menteeId != currentUserId && currentUserRole != "Admin" && currentUserRole != "Mentor")
                {
                    return Forbid();
                }

                var cv = await _unitOfWork.CVs.GetCVByIdAsync(cvId);
                if (cv == null || cv.UserId != menteeId)
                {
                    return NotFound();
                }

                var comments = await _unitOfWork.CVs.GetCVCommentsAsync(cvId);
                return Ok(_mapper.Map<IEnumerable<CVCommentDto>>(comments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for CV {CVId} of mentee {MenteeId}", cvId, menteeId);
                return StatusCode(500, "An error occurred while retrieving comments");
            }
        }

        // GET: api/cvs/mentee/{menteeId}/current
        [HttpGet("mentee/{menteeId}/current")]
        public async Task<ActionResult<MenteeCVDto>> GetCurrentCV(int menteeId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // Verify permissions if user is requesting current CV for someone else
                if (menteeId != currentUserId && currentUserRole != "Admin" && currentUserRole != "Mentor")
                {
                    return Forbid();
                }

                var cv = await _unitOfWork.CVs.GetCurrentCVAsync(menteeId);

                if (cv == null)
                {
                    return NotFound();
                }

                return Ok(_mapper.Map<MenteeCVDto>(cv));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current CV for mentee {MenteeId}", menteeId);
                return StatusCode(500, "An error occurred while retrieving the current CV");
            }
        }
    }
}