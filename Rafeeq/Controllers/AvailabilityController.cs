using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Rafeeq.DTOs.Availability;
using Rafeeq.Services.Availability;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiring authentication for all endpoints
    public class AvailabilityController : ControllerBase
    {
        private readonly AvailabilityService _availabilityService;
        private readonly ILogger<AvailabilityController> _logger;

        public AvailabilityController(AvailabilityService availabilityService, ILogger<AvailabilityController> logger)
        {
            _availabilityService = availabilityService;
            _logger = logger;
        }

        // GET: api/availability/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserAvailability(int userId)
        {
            try
            {
                _logger.LogInformation($"GetUserAvailability called for userId: {userId}");

                // Log authentication information
                var identity = User.Identity;
                _logger.LogInformation($"Is user authenticated: {identity?.IsAuthenticated}");
                _logger.LogInformation($"Authentication type: {identity?.AuthenticationType}");

                var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                _logger.LogInformation($"NameIdentifier claim: {nameIdentifierClaim?.Value ?? "not found"}");

                var result = await _availabilityService.GetUserAvailabilityAsync(userId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving availability");
                return StatusCode(500, new { success = false, message = "Error retrieving availability", error = ex.Message });
            }
        }

      
        [HttpGet("test")]
        public IActionResult Test()
        {
            try
            {
                // Check if user is authenticated
                if (User.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized(new { success = false, message = "Not authenticated" });
                }

                // Get claims information
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var name = User.FindFirst(ClaimTypes.Name)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                return Ok(new
                {
                    success = true,
                    message = "Authentication working",
                    userId = userId,
                    name = name,
                    role = role,
                    allClaims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/availability
        [HttpPost]
        public async Task<IActionResult> AddAvailability(CreateAvailabilityDto dto)
        {
            try
            {
                _logger.LogInformation($"AddAvailability called with UserId: {dto.UserId}, DayOfWeek: {dto.DayOfWeek}");

                // Log authentication information
                _logger.LogInformation($"Is user authenticated: {User.Identity?.IsAuthenticated}");
                var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                _logger.LogInformation($"NameIdentifier claim: {nameIdentifierClaim?.Value ?? "not found"}");

                // Special case handling to make debugging easier
                if (User.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "User not authenticated",
                        details = "Please ensure you're sending the token correctly as 'Bearer {token}' in the Authorization header"
                    });
                }

                // Security check - ensure current user can only add availability for themselves
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                _logger.LogInformation($"Current user ID from token: {currentUserId}, Role: {userRole}");

                if (currentUserId == 0)
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });

                if (dto.UserId != currentUserId && userRole != "Admin")
                {
                    return StatusCode(403, new { success = false, message = "You can only add availability for your own account" });
                }

                // Continue with adding availability
                var result = await _availabilityService.AddAvailabilityAsync(dto);

                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });

                return CreatedAtAction(
                    nameof(GetUserAvailability),
                    new { userId = dto.UserId },
                    new { success = true, message = "Availability added successfully", data = result.Data }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding availability");
                return StatusCode(500, new { success = false, message = "Error adding availability", error = ex.Message });
            }
        }

        // PUT: api/availability/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAvailability(int id, UpdateAvailabilityDto dto)
        {
            try
            {
                // Get current user ID from claims
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (currentUserId == 0)
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });

                var result = await _availabilityService.UpdateAvailabilityAsync(id, dto, currentUserId);

                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });

                return Ok(new { success = true, message = "Availability updated successfully", data = result.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating availability", error = ex.Message });
            }
        }

        // DELETE: api/availability/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            try
            {
                // Get current user ID from claims
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (currentUserId == 0)
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });

                var result = await _availabilityService.DeleteAvailabilityAsync(id, currentUserId);

                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });

                return Ok(new { success = true, message = "Availability deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting availability", error = ex.Message });
            }
        }
    }
}
