using Microsoft.AspNetCore.SignalR;
using Rafeeq.DTOs.Chat;
using Rafeeq.DTOs.Notifications;
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

        public async Task NotifyAllMessagesRead(int bookingId, int userId)
        {
            await _chatHubContext.Clients.Group($"booking-{bookingId}")
                .SendAsync("AllMessagesRead", userId);
        }

        public async Task NotifyTypingIndicator(int bookingId, object typingData)
        {
            await _chatHubContext.Clients.Group($"booking-{bookingId}")
                .SendAsync("UserTyping", typingData);
        }

        public async Task NotifyMessageDeleted(int bookingId, int messageId)
        {
            await _chatHubContext.Clients.Group($"booking-{bookingId}")
                .SendAsync("MessageDeleted", messageId);
        }

      
        public async Task SendNotificationToUser(int userId, NotificationDto notification)
        {
            await _chatHubContext.Clients.User(userId.ToString())
                .SendAsync("ReceiveNotification", notification);
        }

        
        public async Task SendNotificationToUsers(IEnumerable<int> userIds, NotificationDto notification)
        {
            var userIdStrings = userIds.Select(id => id.ToString()).ToList();
            await _chatHubContext.Clients.Users(userIdStrings)
                .SendAsync("ReceiveNotification", notification);
        }

       
        public async Task UpdateUnreadNotificationCount(int userId, int unreadCount)
        {
            await _chatHubContext.Clients.User(userId.ToString())
                .SendAsync("UpdateUnreadCount", unreadCount);
        }

      
        public async Task NotifyAllNotificationsRead(int userId)
        {
            await _chatHubContext.Clients.User(userId.ToString())
                .SendAsync("AllNotificationsRead");
        }

        
        public async Task SendBookingNotification(int bookingId, NotificationDto notification, int? excludeUserId = null)
        {
            var group = $"booking-{bookingId}";

            if (excludeUserId.HasValue)
            {
                await _chatHubContext.Clients.GroupExcept(group, excludeUserId.ToString())
                    .SendAsync("ReceiveNotification", notification);
            }
            else
            {
                await _chatHubContext.Clients.Group(group)
                    .SendAsync("ReceiveNotification", notification);
            }
        }

        
        public async Task SendSystemNotification(NotificationDto notification)
        {
            await _chatHubContext.Clients.All
                .SendAsync("ReceiveSystemNotification", notification);
        }

        public async Task NotifyNewChatMessage(int userId, int bookingId, string senderName, string messagePreview)
        {
            var chatNotification = new
            {
                Type = "chat",
                BookingId = bookingId,
                SenderName = senderName,
                MessagePreview = messagePreview,
                Timestamp = DateTime.UtcNow
            };

            await _chatHubContext.Clients.User(userId.ToString())
                .SendAsync("ReceiveNotification", chatNotification);
        }
    }
}
