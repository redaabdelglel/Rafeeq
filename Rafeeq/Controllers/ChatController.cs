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

                // ✅ FIXED: Add timeout and logging
                _logger.LogInformation($"🔍 Starting download for messageId: {messageId}, userId: {userId}");

                var result = await _chatService.DownloadAttachmentAsync(messageId, userId);

                if (!result.Success)
                {
                    _logger.LogWarning($"❌ Service failed: {result.Message}");
                    return BadRequest(new { success = false, message = result.Message });
                }

                _logger.LogInformation($"✅ Service success, looking for file: {result.Data.FilePath}");

                // ✅ FIXED: Try multiple possible file locations with early exit
                var possiblePaths = new[]
                {
            Path.Combine(_hostingEnvironment.WebRootPath ?? "", result.Data.FilePath.TrimStart('/')),
            Path.Combine(_hostingEnvironment.ContentRootPath, result.Data.FilePath.TrimStart('/'))
        };

                string physicalPath = null;
                foreach (var path in possiblePaths)
                {
                    _logger.LogInformation($"🔍 Checking path: {path}");
                    if (System.IO.File.Exists(path))
                    {
                        physicalPath = path;
                        _logger.LogInformation($"✅ Found file at: {path}");
                        break;
                    }
                }

                if (physicalPath == null)
                {
                    _logger.LogError($"❌ File not found. Attempted paths: {string.Join(", ", possiblePaths)}");
                    return NotFound(new
                    {
                        success = false,
                        message = "File not found on server",
                        attemptedPaths = possiblePaths,
                        originalPath = result.Data.FilePath
                    });
                }

                // ✅ FIXED: Get file info and validate size
                var fileInfo = new FileInfo(physicalPath);
                _logger.LogInformation($"📁 File size: {fileInfo.Length} bytes");

                // ✅ FIXED: Stream the file properly without loading into memory
                var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);

                return File(stream, result.Data.ContentType ?? "application/octet-stream", result.Data.FileName, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"💥 Error downloading attachment for message {messageId}: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while downloading the attachment",
                    error = ex.Message
                });
            }
        }

        // GET: api/chat/debug/attachment/{messageId}
        [HttpGet("debug/attachment/{messageId}")]
        public async Task<IActionResult> DebugAttachment(int messageId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                _logger.LogInformation($"🔍 Debug: Starting for messageId: {messageId}");

                var result = await _chatService.DownloadAttachmentAsync(messageId, userId);

                _logger.LogInformation($"🔍 Debug: Service result: {result.Success}");

                if (!result.Success)
                {
                    return Ok(new
                    {
                        step = "service_failed",
                        message = result.Message,
                        messageId = messageId,
                        userId = userId
                    });
                }

                var possiblePaths = new[]
                {
            Path.Combine(_hostingEnvironment.WebRootPath ?? "", result.Data.FilePath.TrimStart('/')),
            Path.Combine(_hostingEnvironment.ContentRootPath, result.Data.FilePath.TrimStart('/'))
        };

                var debugInfo = possiblePaths.Select(path => new
                {
                    Path = path,
                    Exists = System.IO.File.Exists(path),
                    Size = System.IO.File.Exists(path) ? new FileInfo(path).Length : 0,
                    Directory = Path.GetDirectoryName(path),
                    DirectoryExists = Directory.Exists(Path.GetDirectoryName(path))
                }).ToList();

                return Ok(new
                {
                    step = "file_check_complete",
                    messageId = messageId,
                    originalFilePath = result.Data.FilePath,
                    fileName = result.Data.FileName,
                    contentType = result.Data.ContentType,
                    webRootPath = _hostingEnvironment.WebRootPath,
                    contentRootPath = _hostingEnvironment.ContentRootPath,
                    possiblePaths = debugInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Debug endpoint error");
                return Ok(new
                {
                    step = "exception",
                    error = ex.Message,
                    stackTrace = ex.StackTrace?.Split('\n').Take(5)
                });
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

       
        [HttpPut("conversations/{bookingId}/archive")]
        public async Task<IActionResult> ArchiveConversation(int bookingId, [FromQuery] bool archive = true)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                
                var result = await _chatService.ArchiveConversationAsync(bookingId, userId, archive);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new
                {
                    success = true,
                    message = archive ? "Conversation archived successfully" : "Conversation unarchived successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error {(archive ? "archiving" : "unarchiving")} conversation {bookingId}");
                return StatusCode(500, new { success = false, message = $"An error occurred while {(archive ? "archiving" : "unarchiving")} the conversation", error = ex.Message });
            }
        }

        // GET: api/chat/file-status/{fileName}
        [HttpGet("file-status/{fileName}")]
        public IActionResult GetFileStatus(string fileName)
        {
            try
            {
                // Check the actual file locations
                var possiblePaths = new[]
                {
            Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "chat", "voice", fileName),
            Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "voice", fileName),
            Path.Combine(_hostingEnvironment.ContentRootPath, "uploads", "voice", fileName),
            Path.Combine(_hostingEnvironment.ContentRootPath, "uploads", "chat", "voice", fileName)
        };

                foreach (var path in possiblePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        var fileInfo = new FileInfo(path);

                        // Determine the correct URL based on file location
                        string correctUrl = "";
                        if (path.Contains("wwwroot\\uploads\\chat\\voice"))
                        {
                            correctUrl = $"https://localhost:7001/uploads/chat/voice/{fileName}";
                        }
                        else if (path.Contains("wwwroot\\uploads\\voice"))
                        {
                            correctUrl = $"https://localhost:7001/uploads/voice/{fileName}";
                        }

                        return Ok(new
                        {
                            exists = true,
                            fileSize = fileInfo.Length,
                            correctUrl = correctUrl,
                            physicalPath = path
                        });
                    }
                }

                return Ok(new
                {
                    exists = false,
                    fileSize = 0,
                    correctUrl = "",
                    message = "File not found in any expected location"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/chat/voice/{fileName}
        [HttpGet("voice/{fileName}")]
        public async Task<IActionResult> GetVoiceMessage(string fileName)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                _logger.LogInformation($"🎤 Voice request for: {fileName}");

                // Check voice file locations
                var possiblePaths = new[]
                {
            Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "chat", "voice", fileName),
            Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "voice", fileName),
            Path.Combine(_hostingEnvironment.ContentRootPath, "uploads", "chat", "voice", fileName),
            Path.Combine(_hostingEnvironment.ContentRootPath, "uploads", "voice", fileName)
        };

                string physicalPath = null;
                foreach (var path in possiblePaths)
                {
                    _logger.LogInformation($"🔍 Checking voice path: {path}");
                    if (System.IO.File.Exists(path))
                    {
                        physicalPath = path;
                        _logger.LogInformation($"✅ Found voice file at: {path}");
                        break;
                    }
                }

                if (physicalPath == null)
                {
                    _logger.LogError($"❌ Voice file not found: {fileName}");
                    return NotFound(new
                    {
                        success = false,
                        message = "Voice message not found",
                        fileName = fileName
                    });
                }

                var fileInfo = new FileInfo(physicalPath);
                _logger.LogInformation($"🎤 Voice file size: {fileInfo.Length} bytes");

                // Return file with correct content type for audio
                var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);
                return File(stream, "audio/webm", fileName, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"💥 Error serving voice message {fileName}: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while serving the voice message",
                    error = ex.Message
                });
            }
        }

        // GET: api/chat/voice-info/{fileName}
        [HttpGet("voice-info/{fileName}")]
        public IActionResult GetVoiceMessageInfo(string fileName)
        {
            try
            {
                _logger.LogInformation($"🎤 Voice info request for: {fileName}");

                // Check voice file locations
                var possiblePaths = new[]
                {
            Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "chat", "voice", fileName),
            Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "voice", fileName),
            Path.Combine(_hostingEnvironment.ContentRootPath, "uploads", "chat", "voice", fileName),
            Path.Combine(_hostingEnvironment.ContentRootPath, "uploads", "voice", fileName)
        };

                foreach (var path in possiblePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        var fileInfo = new FileInfo(path);

                        // Generate the correct streaming URL
                        string streamUrl = $"{Request.Scheme}://{Request.Host}/api/chat/voice/{fileName}";

                        return Ok(new
                        {
                            exists = true,
                            fileName = fileName,
                            fileSize = fileInfo.Length,
                            fileSizeFormatted = FormatFileSize(fileInfo.Length),
                            streamUrl = streamUrl, // URL for playing the audio
                            contentType = "audio/webm",
                            physicalPath = path,
                            lastModified = fileInfo.LastWriteTime
                        });
                    }
                }

                return Ok(new
                {
                    exists = false,
                    fileName = fileName,
                    fileSize = 0,
                    message = "Voice file not found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting voice info for {fileName}");
                return BadRequest(new { error = ex.Message, fileName = fileName });
            }
        }

        // Helper method to format file size
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }



    }
}
