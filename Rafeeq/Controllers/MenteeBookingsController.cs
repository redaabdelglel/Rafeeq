using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Bookings;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Rafeeq.DTOs.Mentee;

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
                var currentUserId = GetCurrentUserId(); 

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
        // GET: api/mentee-bookings/pending
        [HttpGet("mentee/{menteeId}/pending")]
        public async Task<ActionResult<IEnumerable<MenteeBookingDto>>> GetPendingBookings()
        {
            try
            {
                var menteeId = GetCurrentUserId();
                var today = DateTime.Today;

                var bookings = await _unitOfWork.Bookings.GetMenteeBookingsAsync(menteeId);
                var PendingBookings = bookings
                    .Where(b => b.StartDateTime <= today && b.Status == "Pending")
                    .OrderBy(b => b.StartDateTime)
                    .ToList();

                return Ok(_mapper.Map<IEnumerable<MenteeBookingDto>>(PendingBookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending's bookings for mentee");
                return StatusCode(500, "An error occurred while retrieving today's bookings");
            }
        }
        // GET: api/mentee-bookings/confirmed
        [HttpGet("mentee/{menteeId}/confirmed")]
        public async Task<ActionResult<IEnumerable<MenteeBookingDto>>> GetConfirmedBookings()
        {
            try
            {
                var menteeId = GetCurrentUserId();
                var today = DateTime.Today;

                var bookings = await _unitOfWork.Bookings.GetMenteeBookingsAsync(menteeId);
                var PendingBookings = bookings
                    .Where(b => b.StartDateTime <= today && b.Status == "confirmed")
                    .OrderBy(b => b.StartDateTime)
                    .ToList();

                return Ok(_mapper.Map<IEnumerable<MenteeBookingDto>>(PendingBookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting confirmed's bookings for mentee");
                return StatusCode(500, "An error occurred while retrieving today's bookings");
            }
        }
        // GET: api/mentee-bookings/today
        [HttpGet("today")]
        public async Task<ActionResult<IEnumerable<MenteeBookingDto>>> GetTodaysBookings()
        {
            try
            {
                var menteeId = GetCurrentUserId();
                var today = DateTime.Today;

                var bookings = await _unitOfWork.Bookings.GetMenteeBookingsAsync(menteeId);
                var todaysBookings = bookings
                    .Where(b => b.StartDateTime == today && b.Status != "Cancelled")
                    .OrderBy(b => b.StartDateTime)
                    .ToList();

                return Ok(_mapper.Map<IEnumerable<MenteeBookingDto>>(todaysBookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's bookings for mentee");
                return StatusCode(500, "An error occurred while retrieving today's bookings");
            }
        }

        // GET: api/mentee-bookings/past
        [HttpGet("past")]
        public async Task<ActionResult<IEnumerable<MenteeBookingDto>>> GetPastBookings()
        {
            try
            {
                var menteeId = GetCurrentUserId();
                var now = DateTime.Now;

                var bookings = await _unitOfWork.Bookings.GetMenteeBookingsAsync(menteeId);
                var pastBookings = bookings
                    .Where(b => b.StartDateTime < now)
                    .OrderByDescending(b => b.StartDateTime)
                    .ToList();

                return Ok(_mapper.Map<IEnumerable<MenteeBookingDto>>(pastBookings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting past bookings for mentee");
                return StatusCode(500, "An error occurred while retrieving past bookings");
            }
        }
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            try
            {
                var menteeId = GetCurrentUserId();
                var booking = await _unitOfWork.Bookings.GetBookingDetailsAsync(id);

                if (booking == null)
                {
                    return NotFound("Booking not found");
                }

                if (booking.MenteeId != menteeId)
                {
                    return Forbid();
                }

                if (booking.StartDateTime < DateTime.Now)
                {
                    return BadRequest("Cannot cancel a session that has already started");
                }

                booking.Status = "Cancelled";
                booking.UpdatedAt = DateTime.Now;

                await _unitOfWork.Bookings.UpdateBookingAsync(booking);
                await _unitOfWork.CompleteAsync();

                return Ok(new { message = "Booking cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
                return StatusCode(500, "An error occurred while cancelling the booking");
            }
        }



        //[HttpPost("mentee/{menteeId}/bookings")]
        ////[ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
        ////[ProducesResponseType(StatusCodes.Status400BadRequest)]
        ////[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        ////[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> CreateBooking(
        //    int menteeId,
        //    [FromQuery] int mentorId,
        //    [FromBody] CreateBookingDTO bookingDto)
        //{
        //    try
        //    {
        //        // Verify the authenticated user matches the menteeId
        //        var currentUserId = GetCurrentUserId();
        //        if (currentUserId != menteeId)
        //        {
        //            return Unauthorized("You can only create bookings for yourself");
        //        }

        //        // Verify mentor exists
        //        var mentor = await _unitOfWork.Mentors.GetMentorByIdAsync(mentorId);
        //        if (mentor == null)
        //        {
        //            return NotFound("Mentor not found");
        //        }

        //        // Map and validate booking
        //        var booking = _mapper.Map<Booking>(bookingDto);
        //        booking.MenteeId = menteeId;
        //        booking.MentorId = mentorId;
        //        booking.CreatedAt = DateTime.Now;
        //        booking.Status = "Scheduled"; // Default status

        //        // Validate booking time
        //        if (booking.StartDateTime < DateTime.Now.AddMinutes(30))
        //        {
        //            return BadRequest("Booking must be scheduled at least 30 minutes in advance");
        //        }

        //        var createdBooking = await _unitOfWork.Bookings.CreateBookingAsync(booking);
        //        await _unitOfWork.CompleteAsync();

        //        var result = _mapper.Map<BookingDto>(createdBooking);

        //        return CreatedAtAction(
        //            nameof(GetBookingDetails),
        //            new { id = createdBooking.BookingId },
        //            result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating booking for mentee {MenteeId} with mentor {MentorId}",
        //            menteeId, mentorId);
        //        return StatusCode(500, "An error occurred while creating the booking");
        //    }
        //}


        //  [HttpPost("mentee/{menteeId}/bookings")]
        //  [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
        //  [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //  [ProducesResponseType(StatusCodes.Status404NotFound)]
        //  public async Task<ActionResult<BookingDto>> CreateBooking(
        //int menteeId,
        //[FromBody] CreateBookingDTO createBookingDto)
        //  {
        //      try
        //      {
        //          // 1. Authorization Check
        //          var currentUserId = GetCurrentUserId();
        //          if (currentUserId != menteeId)
        //          {
        //              return Unauthorized("You can only create bookings for yourself");
        //          }

        //          // 2. Validate Input
        //          if (!ModelState.IsValid)
        //          {
        //              return BadRequest(ModelState);
        //          }

        //          // 3. Verify Mentor Exists
        //          var mentor = await _unitOfWork.Mentors.GetMentorByIdAsync(createBookingDto.MentorId);
        //          if (mentor == null)
        //          {
        //              return NotFound($"Mentor with ID {createBookingDto.MentorId} not found");
        //          }

        //          // 4. Validate Booking Times
        //          if (createBookingDto.StartDateTime >= createBookingDto.EndDateTime)
        //          {
        //              return BadRequest("End time must be after start time");
        //          }

        //          if (createBookingDto.StartDateTime < DateTime.UtcNow.AddMinutes(30))
        //          {
        //              return BadRequest("Bookings must be made at least 30 minutes in advance");
        //          }

        //          // 5. Check Slot Availability
        //          var isSlotAvailable = await _unitOfWork.Bookings.IsSlotAvailableAsync(
        //              createBookingDto.MentorId,
        //              createBookingDto.StartDateTime,
        //              createBookingDto.EndDateTime);

        //          if (!isSlotAvailable)
        //          {
        //              return BadRequest("The selected time slot is already booked");
        //          }

        //          // 6. Create Booking
        //          var booking = _mapper.Map<Booking>(createBookingDto);
        //          booking.MenteeId = menteeId; // From route parameter
        //          booking.CreatedAt = DateTime.UtcNow;
        //          booking.Status = "Scheduled";

        //          var createdBooking = await _unitOfWork.Bookings.CreateBookingAsync(booking);
        //          await _unitOfWork.CompleteAsync();

        //          // 7. Return Response
        //          return CreatedAtAction(
        //              nameof(GetBookingDetails),
        //              new { id = createdBooking.BookingId },
        //              _mapper.Map<BookingDto>(createdBooking));
        //      }
        //      catch (Exception ex)
        //      {
        //          _logger.LogError(ex, "Error creating booking for mentee {MenteeId}", menteeId);
        //          return StatusCode(500, "An error occurred while creating the booking");
        //      }
        //  }


        [HttpPost("mentee/{menteeId}")]
        [ProducesResponseType(typeof(BookingDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> CreateBooking(
           int menteeId,
           [FromBody] CreateBookingDTO bookingDto)
        {
            try
            {
                // Authorization
                var currentUserId = GetCurrentUserId();
                if (currentUserId != menteeId)
                    return Unauthorized("You can only create bookings for yourself");

                // Validation
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (bookingDto.StartDateTime >= bookingDto.EndDateTime)
                    return BadRequest("End time must be after start time");

                

                // Availability check
                var isAvailable = await _unitOfWork.Mentors.IsTimeSlotAvailableAsync(
                    bookingDto.MentorId,
                    bookingDto.StartDateTime,
                    bookingDto.EndDateTime);

                if (!isAvailable)
                {
                    var alternatives = await _unitOfWork.Mentors.GetMentorAvailabilityAsync(
                        bookingDto.MentorId, 3);
                    return Conflict(new
                    {
                        Message = "Time slot not available",
                        Alternatives = alternatives
                    });
                }

                // Create booking
                var booking = new Booking
                {
                    MenteeId = menteeId,
                    MentorId = bookingDto.MentorId,
                    StartDateTime = bookingDto.StartDateTime,
                    EndDateTime = bookingDto.EndDateTime,
                    SessionType = bookingDto.SessionType,
                    TotalAmount = bookingDto.TotalAmount,
                    Status = "Scheduled",
                    CreatedAt = DateTime.UtcNow
                };

                var createdBooking = await _unitOfWork.Bookings.CreateBookingAsync(booking);
                return CreatedAtAction(
                    nameof(GetBookingDetails),
                    new { id = createdBooking.BookingId },
                    _mapper.Map<BookingDto>(createdBooking));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, "Error creating booking");
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

                // Manual update for critical fields
                if (!string.IsNullOrEmpty(updateBookingDTO.GoogleMeetLink))
                {
                    booking.GoogleMeetLink = updateBookingDTO.GoogleMeetLink;
                }
                if (!string.IsNullOrEmpty(updateBookingDTO.Status))
                {
                    booking.Status = updateBookingDTO.Status;
                }
                if (updateBookingDTO.TotalAmount.HasValue)
                {
                    booking.TotalAmount = updateBookingDTO.TotalAmount;
                }

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
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateBooking(int id, UpdateBookingStatusDto updateBookingDTO)
        //{
        //    try
        //    {
        //        var booking = await _unitOfWork.Bookings.GetBookingDetailsAsync(id);
        //        if (booking == null)
        //        {
        //            return NotFound();
        //        }

        //        // Verify the current user is either the mentee or mentor of this booking
        //        var currentUserId = GetCurrentUserId();
        //        if (currentUserId != booking.MenteeId && currentUserId != booking.MentorId)
        //        {
        //            return Forbid();
        //        }

        //        _mapper.Map(updateBookingDTO, booking);
        //        booking.UpdatedAt = DateTime.Now;

        //        await _unitOfWork.Bookings.UpdateBookingAsync(booking);

        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating booking {BookingId}", id);
        //        return StatusCode(500, "An error occurred while updating the booking");
        //    }
        //}

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

