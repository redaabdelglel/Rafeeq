using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Hosting;
using Rafeeq.DTOs.Chat;
using Rafeeq.Hubs;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;


namespace Rafeeq.Services.Chat
{
    public class ChatService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly string _uploadsBasePath;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatService(
    UnitOfWorkManager unitOfWork,
    IMapper mapper,
    IHubContext<ChatHub> chatHubContext,
    IWebHostEnvironment environment,
    IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _chatHubContext = chatHubContext;
            _hostEnvironment = environment;
            _httpContextAccessor = httpContextAccessor;

            try
            {
             
                _uploadsBasePath = Path.Combine(environment.ContentRootPath, "uploads");

              
                if (!Directory.Exists(_uploadsBasePath))
                {
                    Directory.CreateDirectory(_uploadsBasePath);
                }

                
                var voiceDir = Path.Combine(_uploadsBasePath, "voice");
                if (!Directory.Exists(voiceDir))
                {
                    Directory.CreateDirectory(voiceDir);
                }

               
                var chatDir = Path.Combine(_uploadsBasePath, "chat");
                if (!Directory.Exists(chatDir))
                {
                    Directory.CreateDirectory(chatDir);
                }
            }
            catch (Exception ex)
            {
                _uploadsBasePath = Path.Combine(Path.GetTempPath(), "Rafeeq", "uploads");

                try
                {
                    if (!Directory.Exists(_uploadsBasePath))
                    {
                        Directory.CreateDirectory(_uploadsBasePath);
                    }
                }
                catch
                {
                    _uploadsBasePath = Path.GetTempPath();
                }
            }
        }


        public async Task<(bool Success, string Message, IEnumerable<ChatMessageDto> Data)> GetChatHistoryAsync(int bookingId, int userId)
        {
            var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
            if (!isAuthorized)
            {
                return (false, "You don't have permission to view this chat", null);
            }

            var messages = await _unitOfWork.ChatRepository.GetMessagesByBookingIdAsync(bookingId);

            var messageDtos = _mapper.Map<IEnumerable<ChatMessageDto>>(messages);

            return (true, "Chat history retrieved successfully", messageDtos);
        }


        // Send a new message
        public async Task<(bool Success, string Message, ChatMessageDto Data)> SendMessageAsync(SendMessageDto dto, int senderId)
        {
            try
            {
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(dto.BookingId, senderId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to send messages in this chat", null);
                }

                var conversation = await _unitOfWork.ChatRepository.GetConversationByBookingIdAsync(dto.BookingId);
                if (conversation == null)
                {
                    var booking = await _unitOfWork.BookingRepository.GetBookingWithParticipantsAsync(dto.BookingId);
                    if (booking == null)
                    {
                        return (false, "Booking not found", null);
                    }

                    conversation = new ChatConversation
                    {
                        BookingId = dto.BookingId,
                        MentorId = booking.MentorId,
                        MenteeId = booking.MenteeId,
                        LastMessageAt = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    conversation = await _unitOfWork.ChatRepository.CreateConversationAsync(conversation);
                }

                var message = new ChatMessage
                {
                    BookingId = dto.BookingId,
                    ConversationId = conversation.ConversationId, 
                    SenderId = senderId,
                    MessageText = dto.MessageText,
                    IsRead = false,
                    SentAt = DateTime.UtcNow
                };

                var savedMessage = await _unitOfWork.ChatRepository.AddMessageAsync(message);

                conversation.LastMessageAt = message.SentAt;
                await _unitOfWork.ChatRepository.UpdateConversationAsync(conversation);

                var completeMessage = await _unitOfWork.ChatRepository.GetMessageByIdAsync(savedMessage.MessageId);

                var messageDto = _mapper.Map<ChatMessageDto>(completeMessage);

                await _chatHubContext.Clients.Group($"booking-{dto.BookingId}")
                    .SendAsync("ReceiveMessage", messageDto);

                return (true, "Message sent successfully", messageDto);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send message: {ex.Message}", null);
            }
        }


        // Mark message as read
        public async Task<(bool Success, string Message)> MarkMessageAsReadAsync(int messageId, int userId)
        {
            var message = await _unitOfWork.ChatRepository.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                return (false, "Message not found");
            }

            var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(message.BookingId.Value, userId);
            if (!isAuthorized)
            {
                return (false, "You don't have permission to access this message");
            }

            if (message.SenderId == userId)
            {
                return (false, "Cannot mark your own message as read");
            }

            var result = await _unitOfWork.ChatRepository.MarkMessageAsReadAsync(messageId);
            if (!result)
            {
                return (false, "Failed to mark message as read");
            }

            await _chatHubContext.Clients.Group($"booking-{message.BookingId}")
                .SendAsync("MessageRead", messageId);

            return (true, "Message marked as read");
        }

        // Get unread message count
        public async Task<(bool Success, string Message, int Count)> GetUnreadMessagesCountAsync(int userId)
        {
            var count = await _unitOfWork.ChatRepository.GetUnreadMessagesCountAsync(userId);
            return (true, "Unread messages count retrieved", count);
        }

        // Upload attachment
        public async Task<(bool Success, string Message, ChatAttachmentDto Data)> UploadAttachmentAsync(
    int bookingId, int senderId, IFormFile file)
        {
            try
            {
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, senderId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to send attachments in this chat", null);
                }

                if (file == null || file.Length == 0)
                {
                    return (false, "No file was uploaded", null);
                }

                var fileName = Path.GetFileName(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var chatDir = Path.Combine(_uploadsBasePath, "chat");
                var filePath = Path.Combine(chatDir, uniqueFileName);

                Directory.CreateDirectory(chatDir);

                var message = new ChatMessage
                {
                    BookingId = bookingId,
                    SenderId = senderId,
                    MessageText = $"Attachment: {fileName}",
                    IsRead = false,
                    SentAt = DateTime.UtcNow
                };

                var savedMessage = await _unitOfWork.ChatRepository.AddMessageAsync(message);

                var attachment = new ChatAttachment
                {
                    MessageId = savedMessage.MessageId,
                    FileName = fileName,
                    FilePath = $"uploads/chat/{uniqueFileName}", 
                    FileSize = (int)file.Length,
                    ContentType = file.ContentType
                };

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var savedAttachment = await _unitOfWork.ChatAttachmentRepository.AddAttachmentAsync(attachment);

                var attachmentDto = _mapper.Map<ChatAttachmentDto>(savedAttachment);

                var completeMessage = await _unitOfWork.ChatRepository.GetMessageByIdAsync(savedMessage.MessageId);
                var messageDto = _mapper.Map<ChatMessageDto>(completeMessage);

                await _chatHubContext.Clients.Group($"booking-{bookingId}")
                    .SendAsync("ReceiveMessage", messageDto);

                return (true, "Attachment uploaded successfully", attachmentDto);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to upload attachment: {ex.Message}", null);
            }
        }
        public async Task<(bool Success, string Message, IEnumerable<ChatConversationDto> Data)> GetUserConversationsAsync(int userId)
        {
            try
            {
                var conversations = await _unitOfWork.ChatRepository.GetConversationsForUserAsync(userId);
                if (conversations == null || !conversations.Any())
                {
                    return (true, "No conversations found", new List<ChatConversationDto>());
                }

                var conversationDtos = _mapper.Map<IEnumerable<ChatConversationDto>>(conversations);

                foreach (var dto in conversationDtos)
                {
                    var messages = await _unitOfWork.ChatRepository.GetMessagesByConversationIdAsync(dto.ConversationId);
                    var lastMessage = messages.FirstOrDefault();
                    if (lastMessage != null)
                    {
                        dto.LastMessage = _mapper.Map<ChatMessageDto>(lastMessage);
                    }

                    dto.UnreadCount = await _unitOfWork.ChatRepository.GetUnreadMessageCountInConversationAsync(dto.ConversationId, userId);
                }

                return (true, "Conversations retrieved successfully", conversationDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to get conversations: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, object Data)> GetConversationParticipantsAsync(int bookingId, int userId)
        {
            try
            {
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to view this conversation", null);
                }

                var booking = await _unitOfWork.BookingRepository.GetBookingWithParticipantsAsync(bookingId);
                if (booking == null)
                {
                    return (false, "Booking not found", null);
                }

                var result = new
                {
                    BookingId = booking.BookingId,
                    SessionType = booking.SessionType,
                    StartDateTime = booking.StartDateTime,
                    EndDateTime = booking.EndDateTime,
                    Status = booking.Status,
                    GoogleMeetLink = booking.GoogleMeetLink,
                    TotalAmount = booking.TotalAmount,
                    Mentor = new
                    {
                        UserId = booking.Mentor.UserId,
                        FullName = booking.Mentor.FullName,
                        ProfilePicture = booking.Mentor.ProfilePicture,
                        HourlyRate = booking.Mentor.HourlyRate
                    },
                    Mentee = new
                    {
                        UserId = booking.Mentee.UserId,
                        FullName = booking.Mentee.FullName,
                        ProfilePicture = booking.Mentee.ProfilePicture
                    }
                };

                return (true, "Conversation participants retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to get conversation participants: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> MarkAllMessagesAsReadAsync(int bookingId, int userId)
        {
            try
            {
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to access this conversation");
                }

                var conversation = await _unitOfWork.ChatRepository.GetConversationByBookingIdAsync(bookingId);
                if (conversation == null)
                {
                    return (false, "Conversation not found");
                }

                var markedCount = await _unitOfWork.ChatRepository.MarkAllMessagesAsReadAsync(conversation.ConversationId, userId);

                if (markedCount > 0)
                {
                    await _chatHubContext.Clients.Group($"booking-{bookingId}")
                        .SendAsync("AllMessagesRead", userId);
                }

                return (true, $"{markedCount} messages marked as read");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to mark messages as read: {ex.Message}");
            }
        }

        // Download a chat attachment
        public async Task<(bool Success, string Message, FileDownloadDto Data)> DownloadAttachmentAsync(int messageId, int userId)
        {
            try
            {
                var attachments = await _unitOfWork.ChatAttachmentRepository.GetAttachmentsForMessageAsync(messageId);
                var attachment = attachments.FirstOrDefault();

                if (attachment == null)
                {
                    return (false, "Attachment not found", null);
                }

                var isAuthorized = await _unitOfWork.ChatAttachmentRepository.IsUserAuthorizedForAttachmentAsync(attachment.AttachmentId, userId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to access this attachment", null);
                }

                var fileInfo = new FileDownloadDto
                {
                    FilePath = attachment.FilePath,
                    FileName = attachment.FileName,
                    ContentType = attachment.ContentType
                };

                return (true, "Attachment retrieved successfully", fileInfo);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to download attachment: {ex.Message}", null);
            }
        }


        // Delete a message 
        public async Task<(bool Success, string Message)> DeleteMessageAsync(int messageId, int userId)
        {
            try
            {
                var message = await _unitOfWork.ChatRepository.GetMessageByIdAsync(messageId);
                if (message == null)
                {
                    return (false, "Message not found");
                }

                if (message.SenderId != userId)
                {
                    return (false, "You can only delete your own messages");
                }

                var result = await _unitOfWork.ChatRepository.DeleteMessageAsync(messageId);
                if (!result)
                {
                    return (false, "Failed to delete message");
                }

                await _chatHubContext.Clients.Group($"booking-{message.BookingId}")
                    .SendAsync("MessageDeleted", messageId);

                return (true, "Message deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to delete message: {ex.Message}");
            }
        }

        // Send typing indicator
        public async Task<(bool Success, string Message)> SendTypingIndicatorAsync(TypingIndicatorDto dto, int userId)
        {
            try
            {
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(dto.BookingId, userId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to access this conversation");
                }

                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found");
                }

                var typingData = new
                {
                    UserId = userId,
                    UserName = user.FullName,
                    BookingId = dto.BookingId,
                    IsTyping = dto.IsTyping
                };

                await _chatHubContext.Clients.Group($"booking-{dto.BookingId}")
     .SendAsync("UserTyping", typingData);
                return (true, dto.IsTyping ? "Typing indicator sent" : "Typing indicator stopped");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send typing indicator: {ex.Message}");
            }
        }

        // Search messages in a conversation
        public async Task<(bool Success, string Message, IEnumerable<ChatMessageDto> Data)> SearchMessagesAsync(
            int bookingId, string query, int limit, int userId)
        {
            try
            {
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to access this conversation", null);
                }

                var conversation = await _unitOfWork.ChatRepository.GetConversationByBookingIdAsync(bookingId);
                if (conversation == null)
                {
                    return (false, "Conversation not found", null);
                }

                var messages = await _unitOfWork.ChatRepository.SearchMessagesAsync(conversation.ConversationId, query, limit);
                if (messages == null || !messages.Any())
                {
                    return (true, "No messages found matching your search", new List<ChatMessageDto>());
                }

                var messageDtos = _mapper.Map<IEnumerable<ChatMessageDto>>(messages);

                return (true, "Search results retrieved successfully", messageDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to search messages: {ex.Message}", null);
            }
        }

        // Edit a message 
        public async Task<(bool Success, string Message, ChatMessageDto Data)> EditMessageAsync(
            int messageId, string newMessageText, int userId)
        {
            try
            {
                var message = await _unitOfWork.ChatRepository.GetMessageByIdAsync(messageId);
                if (message == null)
                {
                    return (false, "Message not found", null);
                }

                if (message.SenderId != userId)
                {
                    return (false, "You can only edit your own messages", null);
                }

                var messageAge = DateTime.UtcNow - message.SentAt;
                if (messageAge.Value.TotalHours > 24) 
                {
                    return (false, "You can only edit messages within 24 hours of sending", null);
                }

                message.MessageText = newMessageText;
                message.IsEdited = true; 

                await _unitOfWork.ChatRepository.UpdateMessageAsync(message);

                var updatedMessageDto = _mapper.Map<ChatMessageDto>(message);
                await _chatHubContext.Clients.Group($"booking-{message.BookingId}")
                    .SendAsync("MessageEdited", updatedMessageDto);

                return (true, "Message edited successfully", updatedMessageDto);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to edit message: {ex.Message}", null);
            }
        }

        // Add a reaction to a message
        public async Task<(bool Success, string Message, object Data)> AddMessageReactionAsync(
            int messageId, string reactionType, int userId)
        {
            try
            {
                var message = await _unitOfWork.ChatRepository.GetMessageByIdAsync(messageId);
                if (message == null)
                {
                    return (false, "Message not found", null);
                }

               
                if (!message.BookingId.HasValue)
                {
                    return (false, "Invalid message - no booking ID found", null);
                }

                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(message.BookingId.Value, userId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to react to this message", null);
                }

                var reaction = new MessageReaction
                {
                    MessageId = messageId,
                    UserId = userId,
                    ReactionType = reactionType,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ChatRepository.AddMessageReactionAsync(reaction);

                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                var responseData = new
                {
                    MessageId = messageId,
                    ReactionType = reactionType,
                    UserId = userId,
                    UserName = user?.FullName ?? "Unknown User"
                };

                await _chatHubContext.Clients.Group($"booking-{message.BookingId}")
                    .SendAsync("MessageReaction", responseData);

                return (true, "Reaction added", responseData);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to add reaction: {ex.Message}", null);
            }
        }


        public async Task<(bool Success, string Message)> RemoveMessageReactionAsync(
            int messageId, string reactionType, int userId)
        {
            try
            {
                var reaction = await _unitOfWork.ChatRepository.GetMessageReactionAsync(messageId, userId, reactionType);
                if (reaction == null)
                {
                    return (false, "Reaction not found");
                }

                var message = await _unitOfWork.ChatRepository.GetMessageByIdAsync(messageId);
                if (message == null)
                {
                    return (false, "Message not found");
                }

                await _unitOfWork.ChatRepository.RemoveMessageReactionAsync(reaction);

                var responseData = new
                {
                    MessageId = messageId,
                    ReactionType = reactionType,
                    UserId = userId
                };

                await _chatHubContext.Clients.Group($"booking-{message.BookingId}")
                    .SendAsync("MessageReactionRemoved", responseData);

                return (true, "Reaction removed");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to remove reaction: {ex.Message}");
            }
        }

       
        public async Task<(bool Success, string Message, ChatMessageDto Data)> UploadVoiceMessageAsync(
            int bookingId, int senderId, IFormFile audioFile)
        {
            try
            {
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, senderId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to send messages in this chat", null);
                }

                if (audioFile == null || audioFile.Length == 0)
                {
                    return (false, "No audio file provided", null);
                }

                var allowedTypes = new[] { "audio/mpeg", "audio/wav", "audio/webm", "audio/ogg" };
                if (!allowedTypes.Contains(audioFile.ContentType))
                {
                    return (false, "Only audio files (mp3, wav, webm, ogg) are allowed", null);
                }

                if (audioFile.Length > 5 * 1024 * 1024)
                {
                    return (false, "Audio file size exceeds the 5MB limit", null);
                }

                var conversation = await _unitOfWork.ChatRepository.GetConversationByBookingIdAsync(bookingId);
                if (conversation == null)
                {
                    return (false, "Chat conversation not found", null);
                }

                var message = new ChatMessage
                {
                    BookingId = bookingId,
                    ConversationId = conversation.ConversationId,
                    SenderId = senderId,
                    MessageText = "[Voice Message]",
                    IsVoiceMessage = true,
                    SentAt = DateTime.UtcNow
                };

                await _unitOfWork.ChatRepository.AddMessageAsync(message);

                var fileName = $"voice_{Guid.NewGuid()}.webm";

                var voiceDir = Path.Combine(_uploadsBasePath, "voice");
                var fullPath = Path.Combine(voiceDir, fileName);

                Directory.CreateDirectory(voiceDir);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                var attachment = new ChatAttachment
                {
                    MessageId = message.MessageId,
                    FilePath = $"uploads/voice/{fileName}", 
                    FileName = fileName,
                    FileSize = (int)audioFile.Length,
                    ContentType = audioFile.ContentType,
                    IsVoiceMessage = true
                };

                await _unitOfWork.ChatAttachmentRepository.AddAttachmentAsync(attachment);

                conversation.LastMessageAt = message.SentAt;
                _unitOfWork.ChatRepository.UpdateConversation(conversation);
                await _unitOfWork.SaveAsync();

                var completeMessage = await _unitOfWork.ChatRepository.GetMessageByIdAsync(message.MessageId);

                var messageDto = _mapper.Map<ChatMessageDto>(completeMessage);

                SetAbsoluteUrlsForAttachments(new[] { messageDto });

                await _chatHubContext.Clients.Group($"booking-{bookingId}")
                    .SendAsync("ReceiveMessage", messageDto);

                return (true, "Voice message sent successfully", messageDto);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send voice message: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, object Data)> GetOnlineStatusAsync(
            int bookingId, int userId)
        {
            try
            {
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to access this conversation", null);
                }

                var booking = await _unitOfWork.BookingRepository.GetBookingWithParticipantsAsync(bookingId);
                if (booking == null)
                {
                    return (false, "Booking not found", null);
                }

                string groupName = $"booking-{bookingId}";
                bool mentorIsOnline = ChatHub.IsUserConnectedToGroup(booking.MentorId.ToString(), groupName);
                bool menteeIsOnline = ChatHub.IsUserConnectedToGroup(booking.MenteeId.ToString(), groupName);

                var responseData = new
                {
                    MentorId = booking.MentorId,
                    MentorName = booking.Mentor?.FullName,
                    MentorIsOnline = mentorIsOnline,

                    MenteeId = booking.MenteeId,
                    MenteeName = booking.Mentee?.FullName,
                    MenteeIsOnline = menteeIsOnline
                };

                return (true, "Online status retrieved successfully", responseData);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to get online status: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, IEnumerable<ChatConversationDto> Data)> GetBookingsAsPotentialConversationsAsync(int userId)
        {
            try
            {
                var bookings = await _unitOfWork.BookingRepository.GetBookingsForUserAsync(userId);
                if (bookings == null || !bookings.Any())
                {
                    return (true, "No bookings found", new List<ChatConversationDto>());
                }

                var conversationDtos = new List<ChatConversationDto>();

                foreach (var booking in bookings)
                {
                    var existingConversation = await _unitOfWork.ChatRepository.GetConversationByBookingIdAsync(booking.BookingId);
                    if (existingConversation != null)
                    {
                        continue; 
                    }

                    var conversationDto = new ChatConversationDto
                    {
                        ConversationId = 0, 
                        BookingId = booking.BookingId,
                        MentorId = booking.MentorId.GetValueOrDefault(),
                        MentorName = booking.Mentor?.FullName ?? "Unknown Mentor",
                        MentorProfilePicture = booking.Mentor?.ProfilePicture ?? "",
                        MenteeId = booking.MenteeId.GetValueOrDefault(),
                        MenteeName = booking.Mentee?.FullName ?? "Unknown Mentee",
                        MenteeProfilePicture = booking.Mentee?.ProfilePicture ?? "",
                       
                        LastMessageAt = booking.StartDateTime ?? DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = booking.CreatedAt ?? DateTime.UtcNow,
                        UnreadCount = 0,
                      
                        SessionType = booking.SessionType ?? "Mentorship", 
                        SessionStatus = booking.Status ?? "Pending" 
                                                                    
                    };

                    conversationDtos.Add(conversationDto);
                }

                return (true, "Potential conversations retrieved successfully", conversationDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to get potential conversations: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, IEnumerable<ChatMessageDto> Data, int TotalMessages)> GetChatHistoryAsync(
            int bookingId, int userId, int page = 1, int pageSize = 20)
        {
            var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
            if (!isAuthorized)
            {
                return (false, "You don't have permission to view this chat", null, 0);
            }

            var result = await _unitOfWork.ChatRepository.GetMessagesByBookingIdWithPaginationAsync(bookingId, page, pageSize);
            var messages = result.Messages;
            var totalCount = result.TotalCount;

            var messageDtos = _mapper.Map<IEnumerable<ChatMessageDto>>(messages);

            return (true, "Chat history retrieved successfully", messageDtos, totalCount);
        }
        public async Task<(bool Success, string Message, IEnumerable<MessageReactionInfoDto> Data)>
            GetMessageReactionsAsync(int messageId, int userId)
        {
            try
            {
                var message = await _unitOfWork.ChatRepository.GetMessageByIdAsync(messageId);
                if (message == null)
                {
                    return (false, "Message not found", null);
                }

                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(message.BookingId.Value, userId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to view reactions for this message", null);
                }

                var reactions = await _unitOfWork.ChatRepository.GetReactionsForMessageAsync(messageId);

                var reactionDtos = _mapper.Map<IEnumerable<MessageReactionInfoDto>>(reactions);

                return (true, "Message reactions retrieved successfully", reactionDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to get message reactions: {ex.Message}", null);
            }
        }


        public async Task<(bool Success, string Message)> ArchiveConversationAsync(int bookingId, int userId, bool archive)
        {
            try
            {
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to access this conversation");
                }

                var conversation = await _unitOfWork.ChatRepository.GetConversationByBookingIdAsync(bookingId);
                if (conversation == null)
                {
                    return (false, "Conversation not found");
                }

                conversation.IsActive = !archive;

                await _unitOfWork.ChatRepository.UpdateConversationAsync(conversation);

                string actionText = archive ? "archived" : "unarchived";
                return (true, $"Conversation successfully {actionText}");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to {(archive ? "archive" : "unarchive")} conversation: {ex.Message}");
            }
        }

        private string GenerateAbsoluteUrl(string relativePath)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return relativePath;

            var scheme = request.Scheme; 
            var host = request.Host.Value; 

            
            if (!relativePath.StartsWith("/"))
            {
                relativePath = "/" + relativePath;
            }

            return $"{scheme}://{host}{relativePath}";
        }
        private void SetAbsoluteUrlsForAttachments(IEnumerable<ChatMessageDto> messageDtos)
        {
            foreach (var messageDto in messageDtos)
            {
                foreach (var attachment in messageDto.Attachments)
                {
                    if (attachment.IsVoiceMessage)
                    {
                      
                        var fileName = Path.GetFileName(attachment.FilePath);
                        attachment.FullUrl = GenerateAbsoluteUrl($"/api/chat/voice/{fileName}");
                    }
                    else
                    {
                       
                        attachment.FullUrl = GenerateAbsoluteUrl(attachment.FilePath);
                    }
                }
            }
        }




    }
}
