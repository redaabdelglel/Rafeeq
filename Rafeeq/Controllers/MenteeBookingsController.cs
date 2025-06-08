using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Bookings;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using Microsoft.AspNetCore.Authorization;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MenteeBookingsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BookingsController> _logger;

        public MenteeBookingsController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<BookingsController> logger)
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

        // GET: api/bookings/mentee/{menteeId}/all
        [HttpGet("mentee/{menteeId}/all")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetMenteeBookings(int menteeId)
        {
            try
            {
                // Verify the requested menteeId matches the authenticated user

                var currentUserId = GetCurrentUserId();
                if (currentUserId != menteeId)
                {
                    return Forbid();
                }

                var bookings = await _unitOfWork.Bookings.GetMenteeBookingsAsync(menteeId);
                return Ok(_mapper.Map<IEnumerable<BookingDto>>(bookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for mentee {MenteeId}", menteeId);
                return StatusCode(500, "An error occurred while retrieving bookings");
            }
        }

        // GET: api/bookings/mentee/{menteeId}/upcoming
        [HttpGet("mentee/{menteeId}/upcoming")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetUpcomingBookings(int menteeId)
        {
            try
            {
                // Verify the requested menteeId matches the authenticated user

                var currentUserId = GetCurrentUserId();
                if (currentUserId != menteeId)
                {
                    return Forbid();
                }

                var bookings = await _unitOfWork.Bookings.GetUpcomingBookingsAsync(menteeId);
                return Ok(_mapper.Map<IEnumerable<BookingDto>>(bookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming bookings for mentee {MenteeId}", menteeId);
                return StatusCode(500, "An error occurred while retrieving upcoming bookings");
            }
        }

        // GET: api/bookings/mentee/{menteeId}/completed
        [HttpGet("mentee/{menteeId}/completed")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetCompletedBookings(int menteeId)
        {
            try
            {
                // Verify the requested menteeId matches the authenticated user
                var currentUserId = GetCurrentUserId(); // For testing

                if (currentUserId != menteeId)
                {
                    return Forbid();
                }

                var bookings = await _unitOfWork.Bookings.GetCompletedBookingsAsync(menteeId);
                return Ok(_mapper.Map<IEnumerable<BookingDto>>(bookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed bookings for mentee {MenteeId}", menteeId);
                return StatusCode(500, "An error occurred while retrieving completed bookings");
            }
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<ActionResult<BookingDto>> CreateBooking(CreateBookingDTO createBookingDTO)
        {
            try
            {
                var userId = GetCurrentUserId();

                var booking = _mapper.Map<Booking>(createBookingDTO);
                booking.MenteeId = userId;

                var createdBooking = await _unitOfWork.Bookings.CreateBookingAsync(booking);

                return CreatedAtAction(nameof(GetBookingDetails),
                    new { id = createdBooking.BookingId },
                    _mapper.Map<BookingDto>(createdBooking));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, "An error occurred while creating the booking");
            }
        }

        // GET: api/bookings/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDetailsDTO>> GetBookingDetails(int id)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetBookingDetailsAsync(id);
                if (booking == null)
                {
                    return NotFound();
                }

                // Verify the current user is either the mentee or mentor of this booking
                var currentUserId = GetCurrentUserId();
                if (currentUserId != booking.MenteeId && currentUserId != booking.MentorId)
                {
                    return Forbid();
                }

                return Ok(_mapper.Map<BookingDetailsDTO>(booking));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking details for {BookingId}", id);
                return StatusCode(500, "An error occurred while retrieving booking details");
            }
        }

        // PUT: api/bookings/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, UpdateBookingStatusDto updateBookingDTO)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetBookingDetailsAsync(id);
                if (booking == null)
                {
                    return NotFound();
                }

                // Verify the current user is either the mentee or mentor of this booking
                var currentUserId = GetCurrentUserId();
                if (currentUserId != booking.MenteeId && currentUserId != booking.MentorId)
                {
                    return Forbid();
                }

                _mapper.Map(updateBookingDTO, booking);
                booking.UpdatedAt = DateTime.Now;

                await _unitOfWork.Bookings.UpdateBookingAsync(booking);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId}", id);
                return StatusCode(500, "An error occurred while updating the booking");
            }
        }

        // POST: api/bookings/{id}/join
        [HttpPost("{id}/join")]
        public async Task<ActionResult<string>> JoinBooking(int id)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetBookingDetailsAsync(id);
                if (booking == null)
                {
                    return NotFound("Booking not found");
                }

                // Verify the current user is either the mentee or mentor of this booking

                var currentUserId = GetCurrentUserId();
                if (currentUserId != booking.MenteeId && currentUserId != booking.MentorId)
                {
                    return Forbid();
                }

                var meetLink = await _unitOfWork.Bookings.GetGoogleMeetLinkAsync(id);
                if (string.IsNullOrEmpty(meetLink))
                {
                    return NotFound("Meeting link not found");
                }

                return Ok(new { meetLink });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining booking {BookingId}", id);
                return StatusCode(500, "An error occurred while joining the meeting");
            }
        }

        // DELETE: api/bookings/{id} (Soft Delete)
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDeleteBooking(int id)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetBookingDetailsAsync(id);
                if (booking == null || booking.IsDeleted.GetValueOrDefault())
                {
                    return NotFound();
                }

                // Verify the current user is either the mentee or mentor of this booking
                var currentUserId = GetCurrentUserId();
                if (currentUserId != booking.MenteeId && currentUserId != booking.MentorId)
                {
                    return Forbid();
                }

                var result = await _unitOfWork.Bookings.SoftDeleteBookingAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                await _unitOfWork.CompleteAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting booking {BookingId}", id);
                return StatusCode(500, "An error occurred while deleting the booking");
            }
        }
    }
}
