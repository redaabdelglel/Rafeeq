using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Contact;
using Rafeeq.Services.Contact;
using Rafeeq.UnitOfWork;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rafeeq.Controllers.Admin
{
    [Route("api/admin/contact")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        private readonly ILogger<AdminContactController> _logger;
        private readonly UnitOfWorkManager _unitOfWork; // Add this field

        public AdminContactController(
            IContactService contactService,
            ILogger<AdminContactController> logger,
            UnitOfWorkManager unitOfWork) // Inject UnitOfWorkManager
        {
            _contactService = contactService;
            _logger = logger;
            _unitOfWork = unitOfWork; // Initialize field
        }

        // GET: api/admin/contact
        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            try
            {
                var result = await _contactService.GetAllMessagesAsync();

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact messages");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving contact messages", error = ex.Message });
            }
        }

        // GET: api/admin/contact/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(int id)
        {
            try
            {
                var result = await _contactService.GetMessageByIdAsync(id);

                if (!result.Success)
                {
                    return NotFound(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting contact message {id}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the contact message", error = ex.Message });
            }
        }

        // PUT: api/admin/contact/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateMessageStatus(int id, [FromBody] UpdateContactStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _contactService.UpdateMessageStatusAsync(id, dto.Status);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for contact message {id}");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the message status", error = ex.Message });
            }
        }

        // POST: api/admin/contact/{id}/respond
        [HttpPost("{id}/respond")]
        public async Task<IActionResult> RespondToMessage(int id, [FromBody] ContactResponseDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _contactService.RespondToMessageAsync(id, dto.ResponseMessage, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error responding to contact message {id}");
                return StatusCode(500, new { success = false, message = "An error occurred while responding to the message", error = ex.Message });
            }
        }

        // DELETE: api/admin/contact/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                var result = await _contactService.DeleteMessageAsync(id);

                if (!result.Success)
                {
                    return NotFound(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting contact message {id}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the contact message", error = ex.Message });
            }
        }
        [HttpGet("test")]
        [AllowAnonymous] // Make this accessible without authentication for testing
        public async Task<IActionResult> TestContactRepository()
        {
            try
            {
                // Direct database access to verify table exists
                var contactMessagesCount = await _unitOfWork.context.Database
                    .ExecuteSqlRawAsync("SELECT COUNT(*) FROM ContactMessages");

                // Try to get data through repository
                var messages = await _unitOfWork.ContactRepository.GetAllAsync(true);

                return Ok(new
                {
                    directSqlCount = contactMessagesCount,
                    repositoryCount = messages?.Count() ?? 0,
                    messages = messages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}