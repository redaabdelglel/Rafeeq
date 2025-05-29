
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Rafeeq.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(int bookingId, string message, int senderId)
        {
            // Here you would normally save the message to your database through your UnitOfWork
            // But for now we'll just broadcast it without saving

            // Broadcast to everyone in this booking's chat group
            await Clients.Group($"booking-{bookingId}").SendAsync("ReceiveMessage", senderId, message);
        }

        public async Task JoinBookingChat(int bookingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
        }

        public async Task LeaveBookingChat(int bookingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
        }
    }
}
