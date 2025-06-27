using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Rafeeq.DTOs.Chat;
using Rafeeq.Services.Chat;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Rafeeq.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHubContext<ChatHub> _chatHubContext;





        public ChatController(
    ChatService chatService,
    ILogger<ChatController> logger,
    IWebHostEnvironment hostingEnvironment,
    IHubContext<ChatHub> chatHubContext) // Add this parameter
        {
            _chatService = chatService;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _chatHubContext = chatHubContext; // Initialize the field
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
        // GET: api/chat/conversations
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.GetUserConversationsAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user conversations");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving conversations", error = ex.Message });
            }
        }

        // GET: api/chat/conversation/{bookingId}/participants
        [HttpGet("conversation/{bookingId}/participants")]
        public async Task<IActionResult> GetConversationParticipants(int bookingId)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.GetConversationParticipantsAsync(bookingId, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversation participants for booking {bookingId}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving conversation participants", error = ex.Message });
            }
        }

        // PUT: api/chat/conversation/{bookingId}/read-all
        [HttpPut("conversation/{bookingId}/read-all")]
        public async Task<IActionResult> MarkAllMessagesAsRead(int bookingId)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.MarkAllMessagesAsReadAsync(bookingId, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking all messages as read for booking {bookingId}");
                return StatusCode(500, new { success = false, message = "An error occurred while marking messages as read", error = ex.Message });
            }
        }
        // GET: api/chat/attachments/{messageId}
        [HttpGet("attachments/{messageId}")]
        public async Task<IActionResult> DownloadAttachment(int messageId)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.DownloadAttachmentAsync(messageId, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                // Get the file from disk
                var physicalPath = Path.Combine(_hostingEnvironment.WebRootPath, result.Data.FilePath.TrimStart('/'));
                if (!System.IO.File.Exists(physicalPath))
                {
                    return NotFound(new { success = false, message = "File not found on server" });
                }

                // Return the file
                var memory = new MemoryStream();
                using (var stream = new FileStream(physicalPath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, result.Data.ContentType, result.Data.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading attachment for message {messageId}");
                return StatusCode(500, new { success = false, message = "An error occurred while downloading the attachment", error = ex.Message });
            }
        }

        // DELETE: api/chat/messages/{messageId}
        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.DeleteMessageAsync(messageId, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message {messageId}");
                return StatusCode(500, new { success = false, message = "An error occurred while deleting message", error = ex.Message });
            }
        }

        // POST: api/chat/typing
        [HttpPost("typing")]
        public async Task<IActionResult> SendTypingIndicator([FromBody] TypingIndicatorDto dto)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.SendTypingIndicatorAsync(dto, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing indicator");
                return StatusCode(500, new { success = false, message = "An error occurred while sending typing indicator", error = ex.Message });
            }
        }

        // GET: api/chat/search/{bookingId}
        [HttpGet("search/{bookingId}")]
        public async Task<IActionResult> SearchMessages(int bookingId, [FromQuery] string query, [FromQuery] int limit = 50)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.SearchMessagesAsync(bookingId, query, limit, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching messages for booking {bookingId}");
                return StatusCode(500, new { success = false, message = "An error occurred while searching messages", error = ex.Message });
            }
        }
        // PUT: api/chat/messages/{messageId}
        [HttpPut("messages/{messageId}")]
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessageDto dto)
        {
            try
            {
                // Validate the DTO
                if (dto.MessageId != messageId)
                {
                    return BadRequest(new { success = false, message = "Message ID mismatch" });
                }

                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.EditMessageAsync(messageId, dto.MessageText, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = "Message edited successfully", data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error editing message {messageId}");
                return StatusCode(500, new { success = false, message = "An error occurred while editing the message", error = ex.Message });
            }
        }

        // POST: api/chat/messages/{messageId}/reaction
        [HttpPost("messages/{messageId}/reaction")]
        public async Task<IActionResult> AddMessageReaction(int messageId, [FromBody] MessageReactionDto dto)
        {
            try
            {
                // Validate the DTO
                if (dto.MessageId != messageId)
                {
                    return BadRequest(new { success = false, message = "Message ID mismatch" });
                }

                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.AddMessageReactionAsync(messageId, dto.ReactionType, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding reaction to message {messageId}");
                return StatusCode(500, new { success = false, message = "An error occurred while adding reaction", error = ex.Message });
            }
        }

        // DELETE: api/chat/messages/{messageId}/reaction
        [HttpDelete("messages/{messageId}/reaction")]
        public async Task<IActionResult> RemoveMessageReaction(int messageId, [FromQuery] string reactionType)
        {
            try
            {
                if (string.IsNullOrEmpty(reactionType))
                {
                    return BadRequest(new { success = false, message = "Reaction type is required" });
                }

                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.RemoveMessageReactionAsync(messageId, reactionType, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = "Reaction removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing reaction from message {messageId}");
                return StatusCode(500, new { success = false, message = "An error occurred while removing the reaction", error = ex.Message });
            }
        }

        // POST: api/chat/voice-message
        [HttpPost("voice-message")]
        public async Task<IActionResult> UploadVoiceMessage([FromForm] int bookingId, IFormFile audioFile)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.UploadVoiceMessageAsync(bookingId, userId, audioFile);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = "Voice message sent successfully", data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading voice message");
                return StatusCode(500, new { success = false, message = "An error occurred while uploading the voice message", error = ex.Message });
            }
        }

        // GET: api/chat/conversation/{bookingId}/online-status
        [HttpGet("conversation/{bookingId}/online-status")]
        public async Task<IActionResult> GetOnlineStatus(int bookingId)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.GetOnlineStatusAsync(bookingId, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting online status for booking {bookingId}");
                return StatusCode(500, new { success = false, message = "An error occurred while getting online status", error = ex.Message });
            }
        }

        // GET: api/chat/potential-conversations
        [HttpGet("potential-conversations")]
        public async Task<IActionResult> GetPotentialConversations()
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.GetBookingsAsPotentialConversationsAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting potential conversations");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving potential conversations", error = ex.Message });
            }
        }
        [HttpGet("debug/signalr-status")]
        public IActionResult GetSignalRStatus()
        {
            bool isConfigured = _chatHubContext != null;

            return Ok(new
            {
                success = true,
                isHubConfigured = isConfigured,
                hubAvailable = "SignalR hub context is available",
                serverTime = DateTime.UtcNow
            });
        }

        

       
        [HttpGet("messages/{messageId}/reactions")]
        public async Task<IActionResult> GetMessageReactions(int messageId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var result = await _chatService.GetMessageReactionsAsync(messageId, userId);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting reactions for message {messageId}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving reactions", error = ex.Message });
            }
        }


    }
}
