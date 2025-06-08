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

namespace Rafeeq.Services.Chat
{
    public class ChatService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly string _uploadsBasePath;

        public ChatService(
            UnitOfWorkManager unitOfWork,
            IMapper mapper,
            IHubContext<ChatHub> chatHubContext,
            IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _chatHubContext = chatHubContext;

            try
            {
                // Handle case when WebRootPath is null
                if (string.IsNullOrEmpty(environment.WebRootPath))
                {
                    // Use ContentRootPath as fallback
                    string contentRoot = environment.ContentRootPath;
                    _uploadsBasePath = Path.Combine(contentRoot, "wwwroot", "uploads", "chat");
                }
                else
                {
                    _uploadsBasePath = Path.Combine(environment.WebRootPath, "uploads", "chat");
                }

                // Ensure directory exists
                if (!Directory.Exists(_uploadsBasePath))
                {
                    Directory.CreateDirectory(_uploadsBasePath);
                }
            }
            catch (Exception ex)
            {
                // Fallback to temporary directory if we can't create the directory
                _uploadsBasePath = Path.Combine(Path.GetTempPath(), "Rafeeq", "uploads", "chat");

                // Try to create the fallback directory
                try
                {
                    if (!Directory.Exists(_uploadsBasePath))
                    {
                        Directory.CreateDirectory(_uploadsBasePath);
                    }
                }
                catch
                {
                    // Last resort: Just use temporary path
                    _uploadsBasePath = Path.GetTempPath();
                }
            }
        }

        // Get chat history for a booking
        public async Task<(bool Success, string Message, IEnumerable<ChatMessageDto> Data)> GetChatHistoryAsync(int bookingId, int userId)
        {
            // Check if user is part of the booking
            var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
            if (!isAuthorized)
            {
                return (false, "You don't have permission to view this chat", null);
            }

            // Get messages
            var messages = await _unitOfWork.ChatRepository.GetMessagesByBookingIdAsync(bookingId);

            // Map to DTOs
            var messageDtos = _mapper.Map<IEnumerable<ChatMessageDto>>(messages);

            return (true, "Chat history retrieved successfully", messageDtos);
        }

        // Send a new message
        public async Task<(bool Success, string Message, ChatMessageDto Data)> SendMessageAsync(SendMessageDto dto, int senderId)
        {
            try
            {
                // Check if user is part of the booking
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(dto.BookingId, senderId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to send messages in this chat", null);
                }

                // Create new message
                var message = new ChatMessage
                {
                    BookingId = dto.BookingId,
                    SenderId = senderId,
                    MessageText = dto.MessageText,
                    IsRead = false,
                    SentAt = DateTime.UtcNow
                };

                // Save to database
                var savedMessage = await _unitOfWork.ChatRepository.AddMessageAsync(message);

                // Get the complete message with sender details
                var completeMessage = await _unitOfWork.ChatRepository.GetMessageByIdAsync(savedMessage.MessageId);

                // Map to DTO
                var messageDto = _mapper.Map<ChatMessageDto>(completeMessage);

                // Notify clients via SignalR
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
            // Get the message
            var message = await _unitOfWork.ChatRepository.GetMessageByIdAsync(messageId);
            if (message == null)
            {
                return (false, "Message not found");
            }

            // Check if user is part of the booking
            var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(message.BookingId.Value, userId);
            if (!isAuthorized)
            {
                return (false, "You don't have permission to access this message");
            }

            // Don't mark own messages as read
            if (message.SenderId == userId)
            {
                return (false, "Cannot mark your own message as read");
            }

            // Mark as read
            var result = await _unitOfWork.ChatRepository.MarkMessageAsReadAsync(messageId);
            if (!result)
            {
                return (false, "Failed to mark message as read");
            }

            // Notify clients via SignalR that message was read
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
                // Check if user is part of the booking
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, senderId);
                if (!isAuthorized)
                {
                    return (false, "You don't have permission to send attachments in this chat", null);
                }

                // Validate file
                if (file == null || file.Length == 0)
                {
                    return (false, "No file was uploaded", null);
                }

                // Create file path
                var fileName = Path.GetFileName(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var filePath = Path.Combine(_uploadsBasePath, uniqueFileName);

                // Create message first
                var message = new ChatMessage
                {
                    BookingId = bookingId,
                    SenderId = senderId,
                    MessageText = $"Attachment: {fileName}",
                    IsRead = false,
                    SentAt = DateTime.UtcNow
                };

                var savedMessage = await _unitOfWork.ChatRepository.AddMessageAsync(message);

                // Create attachment
                var attachment = new ChatAttachment
                {
                    MessageId = savedMessage.MessageId,
                    FileName = fileName,
                    FilePath = $"/uploads/chat/{uniqueFileName}", // Store as relative URL
                    FileSize = (int)file.Length,
                    ContentType = file.ContentType
                };

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Save attachment to database
                var savedAttachment = await _unitOfWork.ChatAttachmentRepository.AddAttachmentAsync(attachment);

                // Map to DTO
                var attachmentDto = _mapper.Map<ChatAttachmentDto>(savedAttachment);

                // Get updated message with attachment
                var completeMessage = await _unitOfWork.ChatRepository.GetMessageByIdAsync(savedMessage.MessageId);
                var messageDto = _mapper.Map<ChatMessageDto>(completeMessage);

                // Notify clients via SignalR
                await _chatHubContext.Clients.Group($"booking-{bookingId}")
                    .SendAsync("ReceiveMessage", messageDto);

                return (true, "Attachment uploaded successfully", attachmentDto);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to upload attachment: {ex.Message}", null);
            }
        }
    }
}
