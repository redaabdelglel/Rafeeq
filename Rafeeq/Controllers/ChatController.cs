using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Chat;
using Rafeeq.Services.Chat;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(ChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        // GET: api/chat/{bookingId}
        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetChatHistory(int bookingId)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.GetChatHistoryAsync(bookingId, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting chat history for booking {bookingId}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving chat history", error = ex.Message });
            }
        }

        // POST: api/chat
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.SendMessageAsync(dto, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = "Message sent successfully", data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new { success = false, message = "An error occurred while sending the message", error = ex.Message });
            }
        }

        // GET: api/chat/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.GetUnreadMessagesCountAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, count = result.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, new { success = false, message = "An error occurred while getting unread count", error = ex.Message });
            }
        }

        // PUT: api/chat/{messageId}/read
        [HttpPut("{messageId}/read")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.MarkMessageAsReadAsync(messageId, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking message {messageId} as read");
                return StatusCode(500, new { success = false, message = "An error occurred while marking message as read", error = ex.Message });
            }
        }

        // POST: api/chat/attachment
        [HttpPost("attachment")]
        public async Task<IActionResult> UploadAttachment([FromForm] int bookingId, IFormFile file)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.UploadAttachmentAsync(bookingId, userId, file);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = "File uploaded successfully", data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment");
                return StatusCode(500, new { success = false, message = "An error occurred while uploading the attachment", error = ex.Message });
            }
        }
    }
}
