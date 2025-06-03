using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Users;
using Rafeeq.UnitOfWork;

namespace Rafeeq.Controllers
{
    // Controllers/MenteeMentorsController.cs
    [Route("api/mentee-mentors")]
    [ApiController]
    public class MenteeMentorsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MenteeMentorsController> _logger;

        public MenteeMentorsController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<MenteeMentorsController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets filtered list of mentors for mentees
        /// </summary>
        /// <param name="filter">Filter criteria for mentors</param>
        /// <returns>List of mentors matching the criteria</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<MentorDto>>> GetMentors([FromQuery] MentorFilterDTO filter)
        {
            try
            {
                _logger.LogInformation("Fetching mentors with filter: {@Filter}", filter);
                var mentors = await _unitOfWork.Mentors.GetMentorsAsync(filter);
                return Ok(_mapper.Map<IEnumerable<MentorDto>>(mentors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching mentors");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Gets detailed profile of a specific mentor
        /// </summary>
        /// <param name="id">Mentor ID</param>
        /// <returns>Mentor profile details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MentorDto>> GetMentorProfile(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid mentor ID: {MentorId}", id);
                    return BadRequest("Invalid mentor ID");
                }

                _logger.LogInformation("Fetching mentor profile for ID: {MentorId}", id);
                var mentor = await _unitOfWork.Mentors.GetMentorProfileAsync(id);

                if (mentor == null)
                {
                    _logger.LogWarning("Mentor not found with ID: {MentorId}", id);
                    return NotFound();
                }

                return Ok(_mapper.Map<MentorDto>(mentor));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching mentor profile for ID: {MentorId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }
    }
}
