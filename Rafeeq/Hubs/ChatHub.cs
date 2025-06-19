using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Rafeeq.Services.Chat;
using Rafeeq.UnitOfWork;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rafeeq.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly ChatService _chatService;

        public ChatHub(UnitOfWorkManager unitOfWork, ChatService chatService)
        {
            _unitOfWork = unitOfWork;
            _chatService = chatService;
        }

        // No longer needed as we'll use the ChatService for message sending
        // public async Task SendMessage(int bookingId, string message, int senderId)
        // {
        //     // Broadcast to everyone in this booking's chat group
        //     await Clients.Group($"booking-{bookingId}").SendAsync("ReceiveMessage", senderId, message);
        // }

        public async Task JoinBookingChat(int bookingId)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Verify user is authorized to join this booking chat
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
                if (!isAuthorized)
                {
                    // Silently fail to avoid exposing that the booking exists
                    return;
                }

                // Add to SignalR group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"booking-{bookingId}");

                // Get user name
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                string userName = user?.FullName ?? "Unknown User";

                // Notify others that user joined
                await Clients.OthersInGroup($"booking-{bookingId}")
                    .SendAsync("UserJoined", userName);
            }
            catch (Exception)
            {
                // Log exception but don't expose details to client
                // _logger.LogError(ex, $"Error in JoinBookingChat: {ex.Message}");
            }
        }

        public async Task LeaveBookingChat(int bookingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"booking-{bookingId}");

            try
            {
                // Get user ID from claims
                var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Get user name
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                string userName = user?.FullName ?? "Unknown User";

                // Notify others that user left
                await Clients.OthersInGroup($"booking-{bookingId}")
                    .SendAsync("UserLeft", userName);
            }
            catch (Exception)
            {
                // Log exception but don't expose details to client
            }
        }
        // Tell the server that a user read all messages in a conversation
        public async Task MarkAllMessagesAsRead(int bookingId)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return;

                // Notify others in the group
                await Clients.OthersInGroup($"booking-{bookingId}")
                    .SendAsync("AllMessagesRead", userId);
            }
            catch (Exception)
            {
                // Log exception but don't expose details to client
            }
        }

    }
}
