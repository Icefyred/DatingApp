using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId){
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups.Include(con => con.Connections).Where(con => con.Connections.Any(x => x.ConnectionId == connectionId)).FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .Include(user => user.Sender)
                .Include(user => user.Recipient)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(x => x.Connections).FirstOrDefaultAsync(x => x.Name == groupName); 
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams){
            //TODO: Unable to complete lesson 241 when trying to project before creating as query to make the select considerably smaller
            //returns an http 500: The LINQ expression 'DbSet<Message>()\r\n......
            var query = _context.Messages
                .OrderByDescending(m => m.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username
                    && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username
                    && u.SenderDelete == false),
                _ => query.Where(u => u.Recipient.UserName ==
                    messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);

        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            //we get the conversation of the users
            var messages = await _context.Messages
                //.Include(user => user.Sender).ThenInclude(photo => photo.Photos)
                //.Include(user => user.Recipient).ThenInclude(photo => photo.Photos) doesnt require the includes() anymore since we use the projectto
                .Where(message => message.Recipient.UserName == currentUsername && message.RecipientDeleted == false
                && message.Sender.UserName == recipientUsername ||
                message.Recipient.UserName == recipientUsername
                && message.Sender.UserName == currentUsername && message.SenderDelete == false
                )
                .OrderBy(message => message.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            //finds if there's any unread messages for the current user that has received from another user
            var unreadMessages = messages.Where(message => message.DateRead == null && message.RecipientUsername == currentUsername).ToList();
            //mark them as received
            if(unreadMessages.Any()){
                foreach (var message in unreadMessages)
                {  
                   message.DateRead = DateTime.UtcNow;
                }
            }
            
            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        /*
        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }*/
    }
}