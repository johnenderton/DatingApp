using System;
using System.Threading.Tasks;
using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            this.tracker = tracker;
        }

        // SignalR or WebSocket cannot send authentication header
        // what can be use is query string which is gonna be config in IdentityServiceExtensions class
        public override async Task OnConnectedAsync()
        {
            var isOnline = await tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);

            if (isOnline)
                // Send new online username to other user
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

            var currentUsers = await tracker.GetOnlineUsers();
            // Send a list of all connected users to user that just connect to hub
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

            if (isOffline)
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}