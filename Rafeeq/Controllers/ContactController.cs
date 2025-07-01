using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Contact;
using Rafeeq.Services.Contact;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IContactService contactService, ILogger<ContactController> logger)
        {
            _contactService = contactService;
            _logger = logger;
        }

        // POST: api/contact
        [HttpPost]
        public async Task<IActionResult> SubmitContactForm([FromBody] CreateContactDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _contactService.SubmitContactFormAsync(dto);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting contact form");
                return StatusCode(500, new { success = false, message = "An error occurred while submitting the form", error = ex.Message });
            }
        }

        // GET: api/contact/status?email=example@email.com
        [HttpGet("status")]
        public async Task<IActionResult> CheckMessageStatus([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { success = false, message = "Email is required" });
                }

                var result = await _contactService.GetMessagesByEmailAsync(email);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking message status");
                return StatusCode(500, new { success = false, message = "An error occurred while checking message status", error = ex.Message });
            }
        }
       
        
        // get all messages
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllMessages()
        {
            _logger.LogInformation("Admin controller: Getting all contact messages");
            try
            {
                var result = await _contactService.GetAllMessagesAsync();

                _logger.LogInformation($"GetAllMessagesAsync result: Success={result.Success}, Message={result.Message}, Data count={(result.Data?.Count() ?? 0)}");

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin controller: Error getting contact messages");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving contact messages", error = ex.Message });
            }
        }



        
        // GET: api/contact/responded?email=xxx
        [HttpGet("responded")]
        public async Task<IActionResult> GetRespondedMessages([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { success = false, message = "Email is required" });

            var result = await _contactService.GetRespondedMessagesByEmailAsync(email);

            if (!result.success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, data = result.Data });
        }

        // get all conversation

        [HttpGet("conversation")]
        public async Task<IActionResult> GetConversation([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { success = false, message = "Email is required" });
                }

                var result = await _contactService.GetFullConversationAsync(email);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the conversation", error = ex.Message });
            }
        }

        // PATCH: api/contact/{id}/status?status=Read
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
        {
            var result = await _contactService.UpdateMessageStatusAsync(id, status);
            if (!result.Success)
            {
                if (result.Message == "Message not found")
                    return NotFound(new { message = result.Message });

                return BadRequest(new { message = result.Message });
            }


            return Ok(new { message = result.Message });
        }



    }
}
