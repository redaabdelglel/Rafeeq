using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Reviews;
using Rafeeq.UnitOfWork;

namespace Rafeeq.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;
        public ReviewsController(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        //GET    /api/reviews/mentor/{mentorId}    # Get reviews for a mentor
        [HttpGet("mentor/{mentorId}")]
        public async Task<IActionResult> GetByMentor(int mentorId)
        {
            var reviews = await _unitOfWork.ReviewRepository.GetReviewsByMentorIdAsync(mentorId);
            if (!reviews.Any())
            {
                return NotFound("No reviews found for this mentor.");
            }

            var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
            return Ok(reviewDtos);
        }
        //GET    /api/reviews/mentee/{menteeId}    # Get reviews by a mentee
        [HttpGet("mentee/{menteeId}")]
        public async Task<IActionResult> GetByMentee(int menteeId)
        {
            var reviews = await _unitOfWork.ReviewRepository.GetReviewsByMenteeIdAsync(menteeId);
            if (!reviews.Any())
            {
                return NotFound("No reviews found for this mentee.");
            }
            var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);
            return Ok(reviewDtos);
        }

        //POST   /api/reviews    # Add a new review
        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto reviewDto)
        {
            if (reviewDto == null)
            {
                return BadRequest("Review data is required.");
            }
            var review = _mapper.Map<CreateReviewDto>(reviewDto);
            var createdReview = await _unitOfWork.ReviewRepository.AddAsync(review);
            if (createdReview == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating review.");
            }
            var createdReviewDto = _mapper.Map<ReviewDto>(createdReview);
            return CreatedAtAction(nameof(GetByMentor), new { mentorId = createdReview.ReviewerId }, createdReviewDto);
        }
    }
}
