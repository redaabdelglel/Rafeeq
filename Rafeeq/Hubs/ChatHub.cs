using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Rafeeq.Services.Chat;
using Rafeeq.UnitOfWork;
using System;
using System.Collections.Concurrent;
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
                var userIdStr = userId.ToString();

                // Verify user is authorized to join this booking chat
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
                if (!isAuthorized)
                {
                    // Silently fail to avoid exposing that the booking exists
                    return;
                }

                // Add to SignalR group
                string groupName = $"booking-{bookingId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                // Track this user-group association
                _userGroupMap.AddOrUpdate(userIdStr,
                    new HashSet<string> { groupName },
                    (key, existingHashSet) =>
                    {
                        existingHashSet.Add(groupName);
                        return existingHashSet;
                    });

                // Get user name
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                string userName = user?.FullName ?? "Unknown User";

                // Notify others that user joined
                await Clients.OthersInGroup(groupName)
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
            string groupName = $"booking-{bookingId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            try
            {
                // Get user ID from claims
                var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userIdStr = userId.ToString();

                // Remove tracking for this group
                if (_userGroupMap.TryGetValue(userIdStr, out var groups))
                {
                    groups.Remove(groupName);
                    if (groups.Count == 0)
                    {
                        _userGroupMap.TryRemove(userIdStr, out _);
                    }
                }

                // Get user name
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                string userName = user?.FullName ?? "Unknown User";

                // Notify others that user left
                await Clients.OthersInGroup(groupName)
                    .SendAsync("UserLeft", userName);
            }
            catch (Exception)
            {
                // Log exception but don't expose details to client
            }
        }

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
        // Send typing indicator
        public async Task SendTypingIndicator(int bookingId, bool isTyping)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return;

                // Get user details
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null) return;

                // Create typing data
                var typingData = new
                {
                    UserId = userId,
                    UserName = user.FullName,
                    BookingId = bookingId,
                    IsTyping = isTyping
                };

                // Notify others in the group
                await Clients.OthersInGroup($"booking-{bookingId}")
                    .SendAsync("UserTyping", typingData);
            }
            catch (Exception)
            {
                // Log exception but don't expose details to client
            }
        }

        // Track user connection status for online indicators
        private static readonly ConcurrentDictionary<string, List<string>> _userConnectionMap =
            new ConcurrentDictionary<string, List<string>>();

        public override async Task OnConnectedAsync()
        {
            // Get user ID from claims
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                // Add connection to user's connections list
                _userConnectionMap.AddOrUpdate(
                    userId,
                    new List<string> { Context.ConnectionId },
                    (key, existingList) =>
                    {
                        existingList.Add(Context.ConnectionId);
                        return existingList;
                    });

                // Broadcast user's online status to relevant conversations
                var bookings = await _unitOfWork.BookingRepository.GetBookingsForUserAsync(int.Parse(userId));
                foreach (var booking in bookings)
                {
                    await Clients.Group($"booking-{booking.BookingId}").SendAsync("UserOnline", userId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Get user ID from claims
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                // Remove this connection ID
                if (_userConnectionMap.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);

                    // If no more connections, remove user from dictionary and notify others
                    if (connections.Count == 0)
                    {
                        _userConnectionMap.TryRemove(userId, out _);

                        // Broadcast user's offline status to relevant conversations
                        var bookings = await _unitOfWork.BookingRepository.GetBookingsForUserAsync(int.Parse(userId));
                        foreach (var booking in bookings)
                        {
                            await Clients.Group($"booking-{booking.BookingId}").SendAsync("UserOffline", userId);
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
        // Add this static field and method to your ChatHub class
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userGroupMap =
            new ConcurrentDictionary<string, HashSet<string>>();

        // Check if a user is in a specific group
        public static bool IsUserConnectedToGroup(string userId, string groupName)
        {
            if (_userGroupMap.TryGetValue(userId, out var groups))
            {
                return groups.Contains(groupName);
            }
            return false;
        }

    }
}
