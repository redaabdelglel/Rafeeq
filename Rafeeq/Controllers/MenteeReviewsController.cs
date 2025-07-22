using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Reviews;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Controllers
{
    [Route("api/mentee-reviews")]
    [ApiController]
    [Authorize]
    public class MenteeReviewsController : ControllerBase
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MenteeReviewsController> _logger;

        public MenteeReviewsController(
            UnitOfWorkManager unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MenteeReviewsController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        // GET: api/mentee-reviews/mentor/{mentorId}
        [HttpGet("mentor/{mentorId}")]
        public async Task<ActionResult<IEnumerable<ReviewDateDto>>> GetMentorReviews(int mentorId)
        {
            try
            {
                var reviews = await _unitOfWork.MenteeReviewsRepository.GetMentorReviewsAsync(mentorId);
                return Ok(_mapper.Map<IEnumerable<ReviewDateDto>>(reviews));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for mentor {MentorId}", mentorId);
                return StatusCode(500, "An error occurred while retrieving reviews");
            }
        }

        // GET: api/mentee-reviews/mentee/{menteeId}
        [HttpGet("mentee/{menteeId}")]
        public async Task<ActionResult<IEnumerable<ReviewDateDto>>> GetMenteeReviews(int menteeId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId != menteeId)
                {
                    return Forbid();
                }

                var reviews = await _unitOfWork.MenteeReviewsRepository.GetMenteeReviewsAsync(menteeId);
                return Ok(_mapper.Map<IEnumerable<ReviewDateDto>>(reviews));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews by mentee {MenteeId}", menteeId);
                return StatusCode(500, "An error occurred while retrieving reviews");
            }
        }

        // GET: api/mentee-reviews/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDateDto>> GetReview(int id)
        {
            try
            {
                var review = await _unitOfWork.MenteeReviewsRepository.GetReviewByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                var currentUserId = GetCurrentUserId();
                if (currentUserId != review.ReviewerId)
                {
                    return Forbid();
                }

                return Ok(_mapper.Map<ReviewDateDto>(review));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review {ReviewId}", id);
                return StatusCode(500, "An error occurred while retrieving the review");
            }
        }

        // POST: api/mentee-reviews
        [HttpPost]
        public async Task<ActionResult<ReviewDateDto>> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (!createReviewDto.ReviewedUserId.HasValue)
                {
                    return BadRequest("ReviewedUserId is required");
                }

                var canReview = await _unitOfWork.MenteeReviewsRepository
                    .CanMenteeReviewMentorAsync(currentUserId, createReviewDto.ReviewedUserId.Value);


                if (createReviewDto.BookingId.HasValue)
                {
                    var existingReview = await _unitOfWork.MenteeReviewsRepository.GetReviewForBookingAsync(createReviewDto.BookingId.Value);
                    if (existingReview != null)
                    {
                        return BadRequest("You've already reviewed this session");
                    }
                }

                var review = _mapper.Map<Review>(createReviewDto);
                review.ReviewerId = currentUserId;
                review.CreatedAt = DateTime.Now;

                var createdReview = await _unitOfWork.MenteeReviewsRepository.CreateReviewAsync(review);
                await _unitOfWork.SaveAsync();

                return CreatedAtAction(
                    nameof(GetReview),
                    new { id = createdReview.ReviewId },
                    _mapper.Map<ReviewDateDto>(createdReview));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, "An error occurred while creating the review");
            }
        }

        // PUT: api/mentee-reviews/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto)
        {
            try
            {
                var review = await _unitOfWork.MenteeReviewsRepository.GetReviewByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                var currentUserId = GetCurrentUserId();
                if (currentUserId != review.ReviewerId)
                {
                    return Forbid();
                }

                review.Rating = updateReviewDto.Rating;
                review.Comment = updateReviewDto.Comment;
                review.UpdatedAt = DateTime.Now;

                await _unitOfWork.MenteeReviewsRepository.UpdateReviewAsync(review);
                await _unitOfWork.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", id);
                return StatusCode(500, "An error occurred while updating the review");
            }
        }

        // DELETE: api/mentee-reviews/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var review = await _unitOfWork.MenteeReviewsRepository.GetReviewByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                var currentUserId = GetCurrentUserId();
                if (currentUserId != review.ReviewerId)
                {
                    return Forbid();
                }

                await _unitOfWork.MenteeReviewsRepository.DeleteReviewAsync(id);
                await _unitOfWork.SaveAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", id);
                return StatusCode(500, "An error occurred while deleting the review");
            }
        }
    }
}