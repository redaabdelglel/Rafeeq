
using Rafeeq.DTOs.Contact;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Services.Contact
{
    public interface IContactService
    {
        Task<(bool Success, string Message)> SubmitContactFormAsync(CreateContactDto dto);
        Task<(bool Success, string Message, IEnumerable<ContactMessageListDto> Data)> GetAllMessagesAsync();
        Task<(bool Success, string Message, ContactMessageDto Data)> GetMessageByIdAsync(int id);
        Task<(bool Success, string Message)> UpdateMessageStatusAsync(int id, string status);
        Task<(bool Success, string Message)> DeleteMessageAsync(int id);
        Task<(bool Success, string Message, IEnumerable<ContactMessageDto> Data)> GetMessagesByEmailAsync(string email);
        Task <(bool success, string Message , int count)> GetMessagesCountAsync(); //for admin
        Task <(bool success , string Message , IEnumerable<ContactMessageDto> Data)> GetRespondedMessagesByEmailAsync(string email); // for user
        Task<(bool Success, string Message)> AddReplyAsync(CreateReplyDto dto, int responderId);
        Task<(bool Success, string Message, ContactConversationDto Data)> GetFullConversationAsync(string email);

    }
}
