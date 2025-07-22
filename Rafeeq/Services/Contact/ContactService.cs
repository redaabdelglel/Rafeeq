

using AutoMapper;
using Microsoft.Extensions.Logging;
using Rafeeq.DTOs.Contact;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Services.Contact
{
    public class ContactService : IContactService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ContactService> _logger;

        public ContactService(UnitOfWorkManager unitOfWork, IMapper mapper, ILogger<ContactService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> SubmitContactFormAsync(CreateContactDto dto)
        {
            try
            {
                var contactMessage = new ContactMessage
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Subject = dto.Subject ?? "General Inquiry",
                    Message = dto.Message,
                    Status = "New",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ContactRepository.AddAsync(contactMessage);
                return (true, "Your message has been sent. We'll get back to you soon!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting contact form");
                return (false, "Failed to submit your message. Please try again later.");
            }
        }

        public async Task<(bool Success, string Message, IEnumerable<ContactMessageListDto> Data)> GetAllMessagesAsync()
        {
            try
            {
                var messages = await _unitOfWork.ContactRepository.GetAllAsync();
                var messageDtos = _mapper.Map<IEnumerable<ContactMessageListDto>>(messages);
                return (true, "Messages retrieved successfully", messageDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contact messages");
                return (false, "Failed to retrieve messages", Enumerable.Empty<ContactMessageListDto>());
            }
        }

        public async Task<(bool Success, string Message, ContactMessageDto Data)> GetMessageByIdAsync(int id)
        {
            try
            {
                var message = await _unitOfWork.ContactRepository.GetByIdAsync(id);
                if (message == null)
                    return (false, "Message not found", null);

                var messageDto = _mapper.Map<ContactMessageDto>(message);
                return (true, "Message retrieved successfully", messageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving message by ID");
                return (false, "Failed to retrieve message", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateMessageStatusAsync(int id, string status)
        {
            try
            {
                var validStatuses = new[] { "New", "Read", "Responded", "Archived" };
                if (!validStatuses.Contains(status))
                    return (false, "Invalid status value");

                var updated = await _unitOfWork.ContactRepository.UpdateStatusAsync(id, status);
                return updated ? (true, "Status updated successfully") : (false, "Message not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message status");
                return (false, "Failed to update status");
            }
        }

        public async Task<(bool Success, string Message)> DeleteMessageAsync(int id)
        {
            try
            {
                var deleted = await _unitOfWork.ContactRepository.DeleteAsync(id);
                return deleted ? (true, "Message deleted successfully") : (false, "Message not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message");
                return (false, "Failed to delete message");
            }
        }

        public async Task<(bool Success, string Message, IEnumerable<ContactMessageDto> Data)> GetMessagesByEmailAsync(string email)
        {
            try
            {
                var messages = await _unitOfWork.ContactRepository.GetByEmailAsync(email);
                var dtoList = _mapper.Map<IEnumerable<ContactMessageDto>>(messages);
                return (true, "Messages retrieved", dtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages by email");
                return (false, "Failed to retrieve messages", Enumerable.Empty<ContactMessageDto>());
            }
        }

        public async Task<(bool success, string Message, int count)> GetMessagesCountAsync()
        {
            try
            {
                var messages = await _unitOfWork.ContactRepository.GetAllAsync();
                var count = messages.Count(m => m.Status == "New");
                return (true, "Count retrieved", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting new messages");
                return (false, "Error retrieving message count", 0);
            }
        }

        public async Task<(bool success, string Message, IEnumerable<ContactMessageDto> Data)> GetRespondedMessagesByEmailAsync(string email)
        {
            try
            {
                var messages = await _unitOfWork.ContactRepository.GetByEmailAsync(email);
                var responded = messages.Where(m => m.Status == "Responded");
                var dtoList = _mapper.Map<IEnumerable<ContactMessageDto>>(responded);
                return (true, "Responded messages retrieved", dtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving responded messages");
                return (false, "Failed to retrieve messages", Enumerable.Empty<ContactMessageDto>());
            }
        }

        public async Task<(bool Success, string Message)> AddReplyAsync(CreateReplyDto dto, int responderId)
        {
            try
            {
                var reply = new ContactReplies
                {
                    MessageId = dto.MessageId,
                    ReplyText = dto.ReplyText,
                    ResponderId = responderId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ContactRepliesRepository.AddReplyAsync(reply);
                return (true, "Reply added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reply");
                return (false, "Failed to add reply");
            }
        }

        public async Task<(bool Success, string Message, ContactConversationDto Data)> GetFullConversationAsync(string email)
        {
            try
            {
                var messages = await _unitOfWork.ContactRepository.GetConversationByEmailAsync(email);
                var replies = await _unitOfWork.ContactRepliesRepository.GetRepliesByEmailAsync(email);

                var messageDtos = _mapper.Map<List<ContactMessageDto>>(messages);
                var replyDtos = _mapper.Map<List<ContactReplyDto>>(replies);

                return (true, "Conversation fetched successfully", new ContactConversationDto
                {
                    Email = email,
                    Messages = messageDtos,
                    Replies = replyDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching full conversation");
                return (false, "Failed to retrieve full conversation", null);
            }
        }
    }
}
