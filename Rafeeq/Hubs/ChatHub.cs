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

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.UserIdentifier;
                var connectionId = Context.ConnectionId;

                if (!string.IsNullOrEmpty(userId))
                {
                    _userConnectionMap.AddOrUpdate(userId,
                        new List<string> { connectionId },
                        (key, existingConnections) =>
                        {
                            existingConnections.Add(connectionId);
                            return existingConnections;
                        });

                    await Groups.AddToGroupAsync(connectionId, $"user-{userId}");

                    var user = await _unitOfWork.UserRepository.GetUserWithRoleAsync(int.Parse(userId));
                    if (user?.Role != null)
                    {
                        await Groups.AddToGroupAsync(connectionId, $"notifications-{user.Role.RoleName}");
                    }

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

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userId = Context.UserIdentifier;
                var connectionId = Context.ConnectionId;

                if (!string.IsNullOrEmpty(userId))
                {
                    if (_userConnectionMap.TryGetValue(userId, out var connections))
                    {
                        connections.Remove(connectionId);
                        if (!connections.Any())
                        {
                            _userConnectionMap.TryRemove(userId, out _);

                            var bookings = await _unitOfWork.BookingRepository.GetBookingsForUserAsync(int.Parse(userId));
                            foreach (var booking in bookings)
                            {
                                await Clients.Group($"booking-{booking.BookingId}").SendAsync("UserOffline", userId);
                            }
                        }
                    }

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

        
        public async Task JoinNotifications()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
                _logger.LogInformation($"User {userId} joined notification updates");
            }
        }

        
        public async Task LeaveNotifications()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
                _logger.LogInformation($"User {userId} left notification updates");
            }
        }

     
        public async Task MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var userId = Context.UserIdentifier;
                if (!string.IsNullOrEmpty(userId))
                {

                    _logger.LogInformation($"Notification {notificationId} marked as read by user {userId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {notificationId} as read");
            }
        }


        public async Task JoinBookingChat(int bookingId)
        {
            try
            {
               
                var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userIdStr = userId.ToString();

                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
                if (!isAuthorized)
                {
                   
                    return;
                }

              
                string groupName = $"booking-{bookingId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

               
                _userGroupMap.AddOrUpdate(userIdStr,
                    new HashSet<string> { groupName },
                    (key, existingHashSet) =>
                    {
                        existingHashSet.Add(groupName);
                        return existingHashSet;
                    });

       
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                string userName = user?.FullName ?? "Unknown User";

               
                await Clients.OthersInGroup(groupName)
                    .SendAsync("UserJoined", userName);
            }
            catch (Exception)
            {
               
            }
        }

        public async Task LeaveBookingChat(int bookingId)
        {
            string groupName = $"booking-{bookingId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            try
            {
               
                var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userIdStr = userId.ToString();

                
                if (_userGroupMap.TryGetValue(userIdStr, out var groups))
                {
                    groups.Remove(groupName);
                    if (groups.Count == 0)
                    {
                        _userGroupMap.TryRemove(userIdStr, out _);
                    }
                }

              
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                string userName = user?.FullName ?? "Unknown User";

                
                await Clients.OthersInGroup(groupName)
                    .SendAsync("UserLeft", userName);
            }
            catch (Exception)
            {
                
            }
        }

        public async Task MarkAllMessagesAsRead(int bookingId)
        {
            try
            {
               
                var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return;

               
                await Clients.OthersInGroup($"booking-{bookingId}")
                    .SendAsync("AllMessagesRead", userId);
            }
            catch (Exception)
            {
             
            }
        }

      
        public async Task SendTypingIndicator(int bookingId, bool isTyping)
        {
            try
            {
                
                var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return;

               
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (user == null) return;

               
                var typingData = new
                {
                    UserId = userId,
                    UserName = user.FullName,
                    BookingId = bookingId,
                    IsTyping = isTyping
                };

               
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
                
                var userId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return;

                
                var isAuthorized = await _unitOfWork.ChatRepository.IsUserInBookingAsync(bookingId, userId);
                if (!isAuthorized) return;

              
                var result = await _chatService.GetChatHistoryAsync(bookingId, userId, page, pageSize);

                if (result.Success)
                {
                   
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

