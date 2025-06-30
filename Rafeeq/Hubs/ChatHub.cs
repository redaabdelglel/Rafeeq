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
        private readonly ILogger<ChatHub> _logger;

        // ✅ EXISTING TRACKING
        private static readonly ConcurrentDictionary<string, List<string>> _userConnectionMap =
            new ConcurrentDictionary<string, List<string>>();
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userGroupMap =
            new ConcurrentDictionary<string, HashSet<string>>();

        public ChatHub(
            UnitOfWorkManager unitOfWork,
            ChatService chatService,
            ILogger<ChatHub> logger)
        {
            _unitOfWork = unitOfWork;
            _chatService = chatService;
            _logger = logger;
        }

        // ✅ UPDATED: Enhanced OnConnectedAsync with notification support
        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.UserIdentifier;
                var connectionId = Context.ConnectionId;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Track user connections
                    _userConnectionMap.AddOrUpdate(userId,
                        new List<string> { connectionId },
                        (key, existingConnections) =>
                        {
                            existingConnections.Add(connectionId);
                            return existingConnections;
                        });

                    // ✅ NEW: Join user to their personal notification group
                    await Groups.AddToGroupAsync(connectionId, $"user-{userId}");

                    // ✅ NEW: Get user role and join role-based notification group
                    var user = await _unitOfWork.UserRepository.GetUserWithRoleAsync(int.Parse(userId));
                    if (user?.Role != null)
                    {
                        await Groups.AddToGroupAsync(connectionId, $"notifications-{user.Role.RoleName}");
                    }

                    // Broadcast user's online status to relevant conversations
                    var bookings = await _unitOfWork.BookingRepository.GetBookingsForUserAsync(int.Parse(userId));
                    foreach (var booking in bookings)
                    {
                        await Clients.Group($"booking-{booking.BookingId}").SendAsync("UserOnline", userId);
                    }

                    _logger.LogInformation($"User {userId} connected with connection {connectionId}. Total connections: {GetConnectionCount()}");
                }
                else
                {
                    _logger.LogWarning($"Connection attempt without user ID: {Context.ConnectionId}");
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during connection setup for {Context.ConnectionId}");
            }
        }

        // ✅ UPDATED: Enhanced OnDisconnectedAsync
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userId = Context.UserIdentifier;
                var connectionId = Context.ConnectionId;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Remove connection from tracking
                    if (_userConnectionMap.TryGetValue(userId, out var connections))
                    {
                        connections.Remove(connectionId);
                        if (!connections.Any())
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

                    // Remove from groups
                    if (_userGroupMap.TryGetValue(userId, out var groups))
                    {
                        foreach (var group in groups.ToList())
                        {
                            await Groups.RemoveFromGroupAsync(connectionId, group);
                            groups.Remove(group);
                        }

                        if (!groups.Any())
                        {
                            _userGroupMap.TryRemove(userId, out _);
                        }
                    }

                    _logger.LogInformation($"User {userId} disconnected. Remaining connections: {GetConnectionCount()}");
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during disconnection for {Context.ConnectionId}");
            }
        }

        // ✅ NEW NOTIFICATION METHODS
        /// <summary>
        /// Join user to notification updates (called from frontend)
        /// </summary>
        public async Task JoinNotifications()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                _logger.LogInformation($"User {userId} joined notification updates");
            }
        }

        /// <summary>
        /// Leave notification updates
        /// </summary>
        public async Task LeaveNotifications()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
                _logger.LogInformation($"User {userId} left notification updates");
            }
        }

        /// <summary>
        /// Mark notification as read (called from frontend)
        /// </summary>
        public async Task MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (!string.IsNullOrEmpty(userId))
                {
                    // Update notification in database
                    // You'll need to implement this in NotificationRepository
                    // await _unitOfWork.NotificationRepository.MarkAsReadAsync(notificationId, int.Parse(userId));

                    _logger.LogInformation($"Notification {notificationId} marked as read by user {userId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {notificationId} as read");
            }
        }

        // ✅ EXISTING CHAT METHODS (keeping your current implementation)
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
                
            }
        }

        public async Task RequestMoreMessages(int bookingId, int page, int pageSize)
        {
            try
            {
                // Get user ID from claims
                var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return;

                // Verify user is authorized to join this booking chat
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
                if (!isAuthorized) return;

                // Get messages through the chat service
                var result = await _chatService.GetChatHistoryAsync(bookingId, userId, page, pageSize);

                if (result.Success)
                {
                    // Send the messages only to the requesting client
                    await Clients.Caller.SendAsync("ReceiveMoreMessages", result.Data, new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalItems = result.TotalMessages,
                        totalPages = (int)Math.Ceiling(result.TotalMessages / (double)pageSize)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error requesting more messages for booking {bookingId}");
            }
        }

        
        public static bool IsUserConnectedToGroup(string userId, string groupName)
        {
            if (_userGroupMap.TryGetValue(userId, out var groups))
            {
                return groups.Contains(groupName);
            }
            return false;
        }

        
        public static int GetConnectionCount()
        {
            return _userConnectionMap?.Count ?? 0;
        }

        
        public static bool IsUserConnected(string userId)
        {
            return _userConnectionMap.ContainsKey(userId) && _userConnectionMap[userId].Any();
        }

        public static List<string> GetOnlineUsersInGroup(string groupName)
        {
            return _userGroupMap
                .Where(kvp => kvp.Value.Contains(groupName))
                .Select(kvp => kvp.Key)
                .ToList();
        }
    }
}

