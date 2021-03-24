using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SignalR
{
    // We want this to be shared among every connection that comes into our server
    // ->  go to applicationServiceExtension
    // -> add this as a singleton
    public class PresenceTracker
    {
        // key value pair to keep track user online or not
        // key: username
        // value: list of connection id, each of them represent a device
        // this will be shared along with anyone connected to server
        // this is not thread safe result
        // Dictionary will be store in memory not database
        private static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();

        // If we have concurrent users trying to update dictionary at the same time
        // we will run into problems
        // to get around this, we need to lock the dictionary
        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;
            // lock the dictionary until the thing inside here is done
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                } else {
                    OnlineUsers.Add(username, new List<string>{connectionId});
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);
                
                OnlineUsers[username].Remove(connectionId);
                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true;
                }
            }
            return Task.FromResult(isOffline);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(u => u.Key).ToArray();
            }
            return Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            lock(OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username);
            }
            return Task.FromResult(connectionIds);
        }
    }
}