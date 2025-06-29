using Microsoft.AspNetCore.SignalR;
using Rafeeq.Extensions;
using Rafeeq.Hubs;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rafeeq.Extensions
{
    public static class SignalRExtensions
    {
        // ✅ EXISTING METHOD
        public static Task<bool> UserExistsInGroupAsync(this IGroupManager groups, string userId, string groupName)
        {
            if (ChatHub.IsUserConnectedToGroup(userId, groupName))
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        // ✅ NEW NOTIFICATION EXTENSIONS
        /// <summary>
        /// Check if a user is currently online (has any active connections)
        /// </summary>
        public static bool IsUserOnline(this IHubContext<ChatHub> hubContext, string userId)
        {
            return ChatHub.IsUserConnected(userId);
        }

        /// <summary>
        /// Get all online users in a group
        /// </summary>
        public static List<string> GetOnlineUsersInGroup(this IHubContext<ChatHub> hubContext, string groupName)
        {
            return ChatHub.GetOnlineUsersInGroup(groupName);
        }

        /// <summary>
        /// Send notification only if user is online, otherwise store for later
        /// </summary>
        public static async Task SendNotificationIfOnline(this IHubContext<ChatHub> hubContext, string userId, object notification)
        {
            if (ChatHub.IsUserConnected(userId))
            {
                await hubContext.Clients.User(userId)
                    .SendAsync("ReceiveNotification", notification);
            }
            // If user is offline, notification will be retrieved when they log in
        }

        /// <summary>
        /// Join user to notification group (for role-based notifications)
        /// </summary>
        public static async Task JoinNotificationGroup(this IHubContext<ChatHub> hubContext, string connectionId, string groupName)
        {
            await hubContext.Groups.AddToGroupAsync(connectionId, $"notifications-{groupName}");
        }

        /// <summary>
        /// Leave notification group
        /// </summary>
        public static async Task LeaveNotificationGroup(this IHubContext<ChatHub> hubContext, string connectionId, string groupName)
        {
            await hubContext.Groups.RemoveFromGroupAsync(connectionId, $"notifications-{groupName}");
        }
    }
}
