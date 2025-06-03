using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Bookings;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using Rafeeq.Services.Bookings;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(
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

        // GET: api/bookings/mentee/{menteeId}
        [HttpGet("mentee/{menteeId}")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetMenteeBookings(int menteeId)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetMenteeBookingsAsync(menteeId);
                return Ok(_mapper.Map<IEnumerable<BookingDTO>>(bookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for mentee {MenteeId}", menteeId);
                return StatusCode(500, "An error occurred while retrieving bookings");
            }
        }

        // GET: api/bookings/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetUpcomingBookings()
        {
            try
            {
                //var userId = GetCurrentUserId();
                var userId = 1;
                var bookings = await _unitOfWork.Bookings.GetUpcomingBookingsAsync(userId);
                return Ok(_mapper.Map<IEnumerable<BookingDTO>>(bookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming bookings");
                return StatusCode(500, "An error occurred while retrieving upcoming bookings");
            }
        }

        // GET: api/bookings/completed
        [HttpGet("completed")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetCompletedBookings()
        {
            try
            {
                //var userId = GetCurrentUserId();
                var userId = 1;
                var bookings = await _unitOfWork.Bookings.GetCompletedBookingsAsync(userId);
                return Ok(_mapper.Map<IEnumerable<BookingDTO>>(bookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting completed bookings");
                return StatusCode(500, "An error occurred while retrieving completed bookings");
            }
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<ActionResult<BookingDTO>> CreateBooking(CreateBookingDTO createBookingDTO)
        {
            try
            {
                //var userId = GetCurrentUserId();
                var userId = 1;
                var booking = _mapper.Map<Booking>(createBookingDTO);
                booking.MenteeId = userId;

                var createdBooking = await _unitOfWork.Bookings.CreateBookingAsync(booking);

                return CreatedAtAction(nameof(GetBookingDetails),
                    new { id = createdBooking.BookingId },
                    _mapper.Map<BookingDTO>(createdBooking));
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
    }
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IGoogleMeetService _meetService;

        public TestController(IGoogleMeetService meetService)
        {
            _meetService = meetService;
        }

        [HttpGet("meet")]
        public async Task<IActionResult> TestMeet()
        {
            try
            {
                var link = await _meetService.CreateMeetingAsync(
                    "Test Meeting",
                    DateTime.Now.AddHours(1),
                    DateTime.Now.AddHours(2),
                    "Test Description");

                return Ok(new { meetLink = link });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}