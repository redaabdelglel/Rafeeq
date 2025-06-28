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
        // Extension method to check if a user exists in a group
        public static Task<bool> UserExistsInGroupAsync(this IGroupManager groups, string userId, string groupName)
        {
            // Use the connection tracking from ChatHub to determine if a user has any connections in the specified group
            if (ChatHub.IsUserConnectedToGroup(userId, groupName))
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
