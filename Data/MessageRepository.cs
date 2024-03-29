using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DTOs;
using Entities;
using Helpers;
using Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            this.mapper = mapper;
            this.context = context;
        }

        public void AddGroup(Group group)
        {
            context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            this.context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this.context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await context.Groups
                .Include(c => c.Connections)
                .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this.context.Messages
                .Include(u => u.Sender)
                .Include(u => u.Recipient)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = this.context.Messages
                .OrderByDescending(m => m.MessageSent)
                .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
                .AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(
                    u => u.RecipientUsername == messageParams.Username 
                    && u.RecipientDeleted == false
                    ),
                "Outbox" => query.Where(
                    u => u.SenderUsername == messageParams.Username 
                    && u.SenderDeleted == false
                    ),
                _ => query.Where(
                    u => u.RecipientUsername == messageParams.Username 
                    && u.DateRead == null && u.RecipientDeleted == false)
            };
            //var messages = query.ProjectTo<MessageDto>(this.mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await context.Groups
                .Include(x => x.Connections) // Get group connection
                .FirstOrDefaultAsync(g => g.Name == groupName);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var mesages = await this.context.Messages
                // .Include(u => u.Sender).ThenInclude(p => p.Photos)
                // .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(
                    m => m.Recipient.UserName == currentUsername && m.Sender.UserName == recipientUsername && m.RecipientDeleted == false
                    ||
                    m.Recipient.UserName == recipientUsername && m.Sender.UserName == currentUsername && m.SenderDeleted == false
                )
                .OrderBy(m => m.MessageSent)
                .ProjectTo<MessageDto>(mapper.ConfigurationProvider) // if we use projection, no need to use include
                .ToListAsync();

            var unreadMessages = mesages.Where(m => m.DateRead == null && m.RecipientUsername == currentUsername).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
                // await this.context.SaveChangesAsync();
            }
            return mesages;
        }

        public void RemoveConnection(Connection connection)
        {
            context.Connections.Remove(connection);
        }

    }
}