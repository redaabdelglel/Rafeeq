using Microsoft.AspNetCore.SignalR;
using Rafeeq.DTOs.Chat;
using Rafeeq.Hubs;
using Rafeeq.Models;
using System.Threading.Tasks;

namespace Rafeeq.Services.Chat
{
    public class SignalRService
    {
        private readonly IHubContext<ChatHub> _chatHubContext;

        public SignalRService(IHubContext<ChatHub> chatHubContext)
        {
            _chatHubContext = chatHubContext;
        }

        public async Task NotifyNewMessage(int bookingId, ChatMessageDto message)
        {
            await _chatHubContext.Clients.Group($"booking-{bookingId}")
                .SendAsync("ReceiveMessage", message);
        }

        public async Task NotifyMessageRead(int bookingId, int messageId)
        {
            await _chatHubContext.Clients.Group($"booking-{bookingId}")
                .SendAsync("MessageRead", messageId);
        }

        public async Task NotifyUserJoined(int bookingId, string userName)
        {
            await _chatHubContext.Clients.Group($"booking-{bookingId}")
                .SendAsync("UserJoined", userName);
        }

        public async Task NotifyUserLeft(int bookingId, string userName)
        {
            await _chatHubContext.Clients.Group($"booking-{bookingId}")
                .SendAsync("UserLeft", userName);
        }
    }
}
