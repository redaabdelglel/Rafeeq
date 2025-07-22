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
        
        public static Task<bool> UserExistsInGroupAsync(this IGroupManager groups, string userId, string groupName)
        {
            if (ChatHub.IsUserConnectedToGroup(userId, groupName))
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public static bool IsUserOnline(this IHubContext<ChatHub> hubContext, string userId)
        {
            return ChatHub.IsUserConnected(userId);
        }

        
        public static List<string> GetOnlineUsersInGroup(this IHubContext<ChatHub> hubContext, string groupName)
        {
            return ChatHub.GetOnlineUsersInGroup(groupName);
        }

        
        public static async Task SendNotificationIfOnline(this IHubContext<ChatHub> hubContext, string userId, object notification)
        {
            if (ChatHub.IsUserConnected(userId))
            {
                await hubContext.Clients.User(userId)
                    .SendAsync("ReceiveNotification", notification);
            }
           
        }

       
        public static async Task JoinNotificationGroup(this IHubContext<ChatHub> hubContext, string connectionId, string groupName)
        {
            await hubContext.Groups.AddToGroupAsync(connectionId, $"notifications-{groupName}");
        }

       
        public static async Task LeaveNotificationGroup(this IHubContext<ChatHub> hubContext, string connectionId, string groupName)
        {
            await hubContext.Groups.RemoveFromGroupAsync(connectionId, $"notifications-{groupName}");
        }
    }
}
