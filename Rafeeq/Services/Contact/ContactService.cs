using AutoMapper;
using Microsoft.Extensions.Logging;
using Rafeeq.DTOs.Contact;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
                _logger.LogInformation("Fetching all contact messages");

                var messages = await _unitOfWork.ContactRepository.GetAllAsync();

                // Log the number of messages retrieved
                _logger.LogInformation($"Retrieved {messages?.Count() ?? 0} messages");

                if (messages == null)
                {
                    return (false, "No messages found", Enumerable.Empty<ContactMessageListDto>());
                }

                try
                {
                    var messageDtos = _mapper.Map<IEnumerable<ContactMessageListDto>>(messages);
                    return (true, "Messages retrieved successfully", messageDtos);
                }
                catch (Exception mapEx)
                {
                    _logger.LogError(mapEx, "Error mapping contact messages to DTOs");
                    return (false, "Failed to process messages", Enumerable.Empty<ContactMessageListDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contact messages");
                return (false, $"Failed to retrieve messages: {ex.Message}", Enumerable.Empty<ContactMessageListDto>());
            }
        }

        public async Task<(bool Success, string Message, ContactMessageDto Data)> GetMessageByIdAsync(int id)
        {
            try
            {
                var message = await _unitOfWork.ContactRepository.GetByIdAsync(id);
                if (message == null)
                {
                    return (false, "Message not found", null);
                }

                var messageDto = _mapper.Map<ContactMessageDto>(message);
                return (true, "Message retrieved successfully", messageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving contact message {id}");
                return (false, "Failed to retrieve message", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateMessageStatusAsync(int id, string status)
        {
            try
            {
                var validStatuses = new[] { "New", "Read", "Responded", "Archived" };
                if (!validStatuses.Contains(status))
                {
                    return (false, "Invalid status value");
                }

                var result = await _unitOfWork.ContactRepository.UpdateStatusAsync(id, status);
                if (!result)
                {
                    return (false, "Message not found");
                }

                return (true, "Message status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for message {id}");
                return (false, "Failed to update message status");
            }
        }

        public async Task<(bool Success, string Message)> RespondToMessageAsync(int id, string response, int responderId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(response))
                {
                    return (false, "Response message is required");
                }

                var result = await _unitOfWork.ContactRepository.AddResponseAsync(id, response, responderId);
                if (!result)
                {
                    return (false, "Message not found");
                }

                return (true, "Response added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error responding to message {id}");
                return (false, "Failed to add response");
            }
        }

        public async Task<(bool Success, string Message)> DeleteMessageAsync(int id)
        {
            try
            {
                var result = await _unitOfWork.ContactRepository.DeleteAsync(id);
                if (!result)
                {
                    return (false, "Message not found");
                }

                return (true, "Message deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message {id}");
                return (false, "Failed to delete message");
            }
        }

        public async Task<(bool Success, string Message, IEnumerable<ContactMessageDto> Data)> GetMessagesByEmailAsync(string email)
        {
            try
            {
                var messages = await _unitOfWork.ContactRepository.GetByEmailAsync(email);
                var messageDtos = _mapper.Map<IEnumerable<ContactMessageDto>>(messages);
                return (true, "Messages retrieved successfully", messageDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving contact messages for email {email}");
                return (false, "Failed to retrieve messages", Enumerable.Empty<ContactMessageDto>());
            }
        }
    }
}
