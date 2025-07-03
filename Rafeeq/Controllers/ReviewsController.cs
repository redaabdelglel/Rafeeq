using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Reviews;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
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

        
        // GET: api/reviews/written-by/{userId}
        [HttpGet("written-by/{userId}")]
        public async Task<IActionResult> GetReviewsWrittenByUser(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid user ID.");

            var reviews = await _unitOfWork.ReviewRepository.GetReviewsWrittenByUserAsync(userId);

            if (reviews == null || !reviews.Any())
                return NotFound("No reviews written by this user.");

            return Ok(reviews);
        }

        // GET: api/reviews/about/{userId}
        [HttpGet("about/{userId}")]
        public async Task<IActionResult> GetReviewsAboutUser(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid user ID.");

            var reviews = await _unitOfWork.ReviewRepository.GetReviewsAboutUserAsync(userId);

            if (reviews == null || !reviews.Any())
                return NotFound("No reviews written about this user.");

            return Ok(reviews);
        }


        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto reviewDto)
        {
            if (reviewDto == null || reviewDto.BookingId <= 0)
                return BadRequest("Invalid review data or booking ID.");

            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(reviewDto.BookingId ?? 0); 
            if (booking == null)
            {
                return NotFound("Booking not found");
            }

            // Booking must exist
            if (booking == null)
                return NotFound("Booking not found.");

            // Check if the session is completed
            if (!string.Equals(booking.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Cannot review. Session is not completed.");

            // Reviewer must be mentee in this booking
            if (reviewDto.ReviewerId != booking.MenteeId)
                return BadRequest("Only the mentee who attended the session can review the mentor.");

            // Reviewed user must be the mentor in this booking
            if (reviewDto.ReviewedUserId != booking.MentorId)
                return BadRequest("Reviewed user must be the mentor of the session.");

            // Prevent duplicate review for same booking
            if (reviewDto.BookingId.HasValue)
            {
                var existingReview = await _unitOfWork.ReviewRepository
                    .GetByBookingIdAsync(reviewDto.BookingId.Value); // .Value extracts the int from int?

                if (existingReview != null)
                {
                    return BadRequest("You've already reviewed this booking");
                }
            }
            var review = new Review
            {
                ReviewerId = reviewDto.ReviewerId,
                ReviewedUserId = reviewDto.ReviewedUserId,
                BookingId = reviewDto.BookingId,
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ReviewRepository.CreateReview(review);
            await _unitOfWork.SaveAsync();

            return Ok("Review created successfully.");
        }

    }
}