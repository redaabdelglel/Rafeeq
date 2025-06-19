// Controllers/MenteeMentorsController.cs
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Users;
using Rafeeq.UnitOfWork;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rafeeq.DTOs.Availability;

namespace Rafeeq.Controllers
{
    [Route("api/mentors")]
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
        /// Gets all mentors
        /// </summary>
        /// <returns>List of all mentors</returns>
        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MentorDto>>> GetAllMentors()
        {
            try
            {
                _logger.LogInformation("Fetching all mentors");
                var mentors = await _unitOfWork.Mentors.GetAllMentorsAsync();
                return Ok(_mapper.Map<IEnumerable<MentorDto>>(mentors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all mentors: {Message} | {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}\n{ex.StackTrace}");
            }
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

                // 1. Log before fetching
                _logger.LogDebug("Attempting to fetch mentor with ID: {MentorId}", id);

                var mentor = await _unitOfWork.Mentors.GetMentorProfileAsync(id);

                // 2. Log raw database results
                if (mentor == null)
                {
                    _logger.LogWarning("Mentor not found with ID: {MentorId}", id);
                    return NotFound();
                }

                _logger.LogDebug("Raw mentor data loaded: {@MentorData}", new
                {
                    mentor.UserId,
                    Availabilities = mentor.Availabilities?.Select(a => new {
                        a.AvailabilityId,
                        a.DayOfWeek,
                        a.StartTime,
                        a.EndTime
                    }) ?? Enumerable.Empty<object>()
                });

                // 3. Log before mapping
                _logger.LogDebug("Attempting to map mentor to DTO");

                var mentorDto = _mapper.Map<MentorDto>(mentor);

                // 4. Log after mapping
                _logger.LogDebug("Mapped mentor DTO: {@MentorDto}", new
                {
                    mentorDto.UserId,
                    Availabilities = mentorDto.Availabilities?.Select(a => new {
                        a.AvailabilityId,
                        a.DayOfWeek,
                        a.StartTime,
                        a.EndTime
                    }) ?? Enumerable.Empty<object>()
                });

                // 5. Log before serialization
                _logger.LogDebug("Attempting to serialize response");

                return Ok(mentorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching mentor profile for ID: {MentorId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
            }
            return userId;
        }

        [HttpGet("mentors/{mentorId}")]
        [ProducesResponseType(typeof(List<AvailableSlotDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetMentorAvailability(
            int mentorId,
            [FromQuery] int daysAhead = 14)
        {
            try
            {
                var availability = await _unitOfWork.Mentors.GetMentorAvailabilityAsync(mentorId, daysAhead);
                if (availability == null || !availability.Any())
                    return NotFound("No availability found for this mentor");

                return Ok(availability);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting availability for mentor {MentorId}", mentorId);
                return StatusCode(500, "Error retrieving availability");
            }
        }
    }
}