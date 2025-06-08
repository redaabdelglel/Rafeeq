using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Bookings;
using Rafeeq.Services.Bookings;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class BookingsController : ControllerBase
    {
        private readonly BookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(BookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        // GET: api/bookings/mentor/{mentorId}
        [HttpGet("mentor/{mentorId}")]
        public async Task<IActionResult> GetMentorBookings(int mentorId)
        {
            try
            {
                // Get current user ID from claims for authorization check
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Check if user is the mentor or an admin
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
                // Get current user ID from claims
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

        // Additional endpoints will be added by Rawan for mentee functionality
    }
}