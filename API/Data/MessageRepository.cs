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

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(user => user.Recipient.UserName == messageParams.Username && user.RecipientDeleted == false),
                "Outbox" => query.Where(user => user.Sender.UserName == messageParams.Username && user.SenderDelete == false),
                _ => query.Where(user => user.Recipient.UserName == messageParams.Username && user.RecipientDeleted == false && user.DateRead == null)
            };
            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);
            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            //we get the conversation of the users
            var messages = await _context.Messages
                .Include(user => user.Sender).ThenInclude(photo => photo.Photos)
                .Include(user => user.Recipient).ThenInclude(photo => photo.Photos)
                .Where(message => message.Recipient.UserName == currentUsername && message.RecipientDeleted == false
                && message.Sender.UserName == recipientUsername ||
                message.Recipient.UserName == recipientUsername
                && message.Sender.UserName == currentUsername && message.SenderDelete == false
                )
                .OrderBy(message => message.MessageSent)
                .ToListAsync();
            //finds if there's any unread messages for the current user that has received from another user
            var unreadMessages = messages.Where(message => message.DateRead == null && message.Recipient.UserName == currentUsername).ToList();
            //mark them as received
            if(unreadMessages.Any()){
                foreach (var message in unreadMessages)
                {  
                   message.DateRead = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
            
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}