using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Bookings;
using Rafeeq.Services.Bookings;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class BookingsController : ControllerBase
    {
        private readonly BookingService _bookingService;
        private readonly GoogleMeetService _googleMeetService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(
            BookingService bookingService,
            GoogleMeetService googleMeetService,
            ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _googleMeetService = googleMeetService;
            _logger = logger;
        }

        // POST: api/bookings/{id}/join
        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinBooking(int id)
        {
            try
            {
                _logger.LogInformation("JoinBooking endpoint called for booking ID: {BookingId}", id);

                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var result = await _bookingService.GetBookingMeetLinkAsync(id, currentUserId, userRole);

                if (!result.Success)
                {
                    _logger.LogWarning("Failed to get meeting link: {Message}", result.Message);
                    return BadRequest(new { success = false, message = result.Message });
                }

                _logger.LogInformation("Successfully retrieved meet link for booking {BookingId}: {Link}", id, result.Data);

                bool isRealLink = !string.IsNullOrEmpty(result.Data) &&
                                  (result.Data.Contains("meet.google.com") ||
                                   result.Data.Contains("zoom.us") ||
                                   result.Data.Contains("teams.microsoft.com") ||
                                   result.Data.Contains("google.com/calendar"));

                return Ok(new
                {
                    success = true,
                    meetLink = result.Data,
                    isRealLink = isRealLink,
                    linkType = isRealLink ? "meeting_link" : "other"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting meeting link for booking {BookingId}", id);
                return StatusCode(500, new { success = false, message = "Error getting meeting link", error = ex.Message });
            }
        }




        // GET: api/bookings/upcoming
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingBookings()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var result = await _bookingService.GetUpcomingBookingsAsync(currentUserId, userRole);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming bookings");
                return StatusCode(500, new { success = false, message = "Error retrieving upcoming bookings", error = ex.Message });
            }
        }

        // GET: api/bookings/completed
        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedBookings()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var result = await _bookingService.GetCompletedBookingsAsync(currentUserId, userRole);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving completed bookings");
                return StatusCode(500, new { success = false, message = "Error retrieving completed bookings", error = ex.Message });
            }
        }

        // PUT: api/bookings/{id}/reschedule
        [HttpPut("{id}/reschedule")]
        public async Task<IActionResult> RescheduleBooking(int id, [FromBody] RescheduleBookingDto rescheduleDto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var result = await _bookingService.RescheduleBookingAsync(id, rescheduleDto, currentUserId, userRole);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = "Booking rescheduled successfully", data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rescheduling booking {id}");
                return StatusCode(500, new { success = false, message = "Error rescheduling booking", error = ex.Message });
            }
        }
        // GET: api/bookings/test-google-meet
        [HttpGet("test-google-meet")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> TestGoogleMeet()
        {
            try
            {
                _logger.LogInformation("TestGoogleMeet endpoint called");

                string meetingName = $"Test Meeting {DateTime.UtcNow.Ticks}";
                DateTime startTime = DateTime.UtcNow.AddHours(1);
                DateTime endTime = DateTime.UtcNow.AddHours(2);
                string description = "Test meeting created through API endpoint";

                _logger.LogInformation("Creating test meeting: {Name}, {Start} - {End}",
                    meetingName, startTime, endTime);

                string meetLink = await _googleMeetService.CreateMeetingAsync(
                    meetingName,
                    startTime,
                    endTime,
                    description);

                _logger.LogInformation("Test result: {Link}", meetLink);

                bool isRealLink = !meetLink.Contains("/error-") &&
                                  !meetLink.Contains("/fallback-") &&
                                  !meetLink.Contains("/mock-");

                return Ok(new
                {
                    success = true,
                    meetLink = meetLink,
                    isRealLink = isRealLink,
                    meetingDetails = new
                    {
                        name = meetingName,
                        startTime = startTime,
                        endTime = endTime,
                        description = description
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestGoogleMeet endpoint");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        [HttpGet("test-simple-event")]
        [Authorize(Roles = "Admin,Mentor")]
        public async Task<IActionResult> TestSimpleEvent()
        {
            try
            {
                _logger.LogInformation("TestSimpleEvent endpoint called");

                string result = await _googleMeetService.CreateSimpleEventAsync(
                    $"Simple Test Event {DateTime.UtcNow.Ticks}",
                    DateTime.UtcNow.AddHours(1),
                    DateTime.UtcNow.AddHours(2),
                    "Testing simple event creation without conferencing");

                return Ok(new { success = true, result = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestSimpleEvent");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        // GET: api/bookings/mentor/{mentorId}
        [HttpGet("mentor/{mentorId}")]
        public async Task<IActionResult> GetMentorBookings(int mentorId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (mentorId != currentUserId && userRole != "Admin")
                {
                    return Forbid("You can only view your own bookings");
                }

                var bookings = await _bookingService.GetMentorBookingsAsync(mentorId);
                return Ok(new { success = true, data = bookings });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving bookings for mentor {mentorId}");
                return StatusCode(500, new { success = false, message = "Error retrieving bookings", error = ex.Message });
            }
        }

        // PUT: api/bookings/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto updateDto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var result = await _bookingService.UpdateBookingStatusAsync(id, updateDto.Status, currentUserId, userRole);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = "Booking status updated successfully", data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for booking {id}");
                return StatusCode(500, new { success = false, message = "Error updating booking status", error = ex.Message });
            }
        }
        // GET: api/bookings/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingDetails(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var result = await _bookingService.GetBookingByIdAsync(id, currentUserId, userRole);

                if (!result.Success)
                {
                    return NotFound(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving booking details for booking {id}");
                return StatusCode(500, new { success = false, message = "Error retrieving booking details", error = ex.Message });
            }
        }

       
        [HttpPut("{id}/meeting-link")]
        public async Task<IActionResult> UpdateMeetingLink(int id, [FromBody] UpdateMeetingLinkDto linkDto)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var result = await _bookingService.UpdateMeetingLinkAsync(id, linkDto.MeetingLink, currentUserId, userRole);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = "Meeting link updated successfully", data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating meeting link for booking {id}");
                return StatusCode(500, new { success = false, message = "Error updating meeting link" });
            }
        }



    }
}
