using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DTOs;
using Entities;
using Extensions;
using Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace SignalR
{
    // SignalR Hub will not access to httpResponse since it does not use it
    public class MessageHub : Hub
    {
        private readonly IMapper mapper;
        private readonly IHubContext<PresenceHub> presenceHub;
        private readonly PresenceTracker tracker;
        private readonly IUnitOfWork unitOfWork;
        public MessageHub(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
        {
            this.unitOfWork = unitOfWork;
            this.tracker = tracker;
            this.presenceHub = presenceHub;
            this.mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            // when login user click on other user profile -> get that username
            // the query string will be defined in message.service.ts
            var otherUser = httpContext.Request.Query["user"].ToString();
            // The group name will be in alphabetical order
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName); // this Groups object is from signalR

            var group = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

            // If there are changes in entity message through function GetMessageThread
            // and the changes might come from DateRead property
            // save changes
            if (unitOfWork.HasChanges()) await unitOfWork.Complete();

            // This one send message to both users connected to hub
            // eventhough either one already have the message
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You Cannot Send Messages To Yourself");

            // Get hold of both our users in the sender and the recipient
            // As we need to populate the messages when we create it
            // And going the other way, we need to return a dto from this as well
            var sender = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) throw new HubException("User Not Found!");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };
            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await tracker.GetConnectionsForUser(recipient.UserName);

                if (connections != null) // user is online but not in the same group
                {
                    await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new { username = sender.UserName, knownAs = sender.KnownAs }
                    );
                }
            }
            this.unitOfWork.MessageRepository.AddMessage(message);

            if (await this.unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }
        }

        private string GetGroupName(string caller, string other)
        {
            // Compare two string
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        // Create couple of private method for adding and removing a user from a group

        // HubCallerContext give us access to username and connectionID
        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
            // When user connect to this hub, they always get new connection id
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                unitOfWork.MessageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);
            if (await unitOfWork.Complete()) return group;

            throw new HubException("Failed to join group!");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            unitOfWork.MessageRepository.RemoveConnection(connection);

            if (await unitOfWork.Complete()) return group;

            throw new HubException("Failed to Remove From Group!");
        }
    }
}