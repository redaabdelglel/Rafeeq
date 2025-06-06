using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.CV;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class CVsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CVsController(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        // GET: api/cvs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CVDTO>>> GetMenteeCVs()
        {
            //var userId = GetCurrentUserId();
            var userId = 2;

            var cvs = await _unitOfWork.CVs.GetMenteeCVsAsync(userId);
            return Ok(_mapper.Map<IEnumerable<CVDTO>>(cvs));
        }

        // POST: api/cvs
        [HttpPost]
        public async Task<ActionResult<CVDTO>> UploadCV([FromForm] UploadCVDTO uploadCVDTO)
        {
            //var userId = GetCurrentUserId();
            var userId = 2;


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
                UserId = userId,
                FilePath = filePath,
                FileName = uploadCVDTO.File.FileName,
                FileSize = (int)uploadCVDTO.File.Length,
                ContentType = uploadCVDTO.File.ContentType,
                UploadDate = DateTime.Now,
                IsActive = true
            };

            var createdCV = await _unitOfWork.CVs.UploadCVAsync(cv);

            return CreatedAtAction(nameof(GetMenteeCVs),
                _mapper.Map<CVDTO>(createdCV));
        }

        // DELETE: api/cvs/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCV(int id)
        {
            var result = await _unitOfWork.CVs.DeleteCVAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // GET: api/cvs/comments/{cvId}
        [HttpGet("comments/{cvId}")]
        public async Task<ActionResult<IEnumerable<CVCommentDto>>> GetCVComments(int cvId)
        {
            var comments = await _unitOfWork.CVs.GetCVCommentsAsync(cvId);
            return Ok(_mapper.Map<IEnumerable<CVCommentDto>>(comments));
        }

        // POST: api/users/upload-cv
        [HttpPost("users/upload-cv")]
        public async Task<ActionResult<CVDTO>> UploadCVAlternate([FromForm] UploadCVDTO uploadCVDTO)
        {
            return await UploadCV(uploadCVDTO);
        }

        // GET: api/users/cv
        [HttpGet("users/cv")]
        public async Task<ActionResult<CVDTO>> GetCurrentCV()
        {
            //var userId = GetCurrentUserId();
            var userId = 2;

            var cv = await _unitOfWork.CVs.GetCurrentCVAsync(userId);
            if (cv == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CVDTO>(cv));
        }
    }

}
