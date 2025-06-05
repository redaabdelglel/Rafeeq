using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Reviews;
using Rafeeq.UnitOfWork;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
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
        // GET: api/reviews/mentor/{mentorid}
        [HttpGet("mentor/{id}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetMentorReviews(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid mentor ID.");
            }

            var reviews = await _unitOfWork.ReviewRepository.GetReviewsByMentorIdAsync(id);

            if (reviews == null || !reviews.Any())
            {
                return NotFound("No reviews found for this mentor.");
            }


            return Ok(_mapper.Map<IEnumerable<ReviewDto>>(reviews));
        }
        // GET: api/reviews/mentee/{menteeid}
        [HttpGet("mentee/{id}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetMenteeReviews(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid mentee ID.");
            }
            var reviews = await _unitOfWork.ReviewRepository.GetReviewsByMenteeIdAsync(id);
            if (reviews == null || !reviews.Any())
            {
                return NotFound("No reviews found for this mentee.");
            }
            return Ok(_mapper.Map<IEnumerable<ReviewDto>>(reviews));

        }
    }
}
