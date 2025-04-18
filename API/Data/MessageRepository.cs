using System;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddGroup(Group group)
    {
        context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Connection?> GetConnection(string connectionId)
    {
        return await context.Connections.FindAsync(connectionId);
    }

    public async Task<Group?> GetGroupForConnection(string connectionId)
    {
        return await context.Groups
            .Include(x => x.Connections)
            .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages.FindAsync(id);
    }

   public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
{
    var query = context.Messages
        .Include(m => m.Sender).ThenInclude(p => p.Photos)
        .Include(m => m.Recipient).ThenInclude(p => p.Photos)
        .OrderByDescending(m => m.MessageSent)
        .AsQueryable();

    query = messageParams.Container switch
    {
        "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username && !u.RecipientDeleted),
        "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && !u.SenderDeleted),
        _ => query.Where(u => u.RecipientUsername == messageParams.Username && !u.RecipientDeleted && u.DateRead == null)
    };

    var pagedMessages = await PagedList<Message>.CreateAsync(
        query,
        messageParams.pageNumber,
        messageParams.PageSize
    );

    var messageDtos = mapper.Map<List<MessageDto>>(pagedMessages);

    return new PagedList<MessageDto>(
        messageDtos,
        pagedMessages.TotalCount,
        pagedMessages.CurrentPage,
        pagedMessages.PageSize
    );
}
    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        var query =  context.Messages
            .Include(m => m.Sender).ThenInclude(p => p.Photos)
            .Include(m => m.Recipient).ThenInclude(p => p.Photos)
            .Where(x =>
                (x.RecipientUsername == currentUsername && x.SenderUsername == recipientUsername && !x.RecipientDeleted)
                ||
                (x.SenderUsername == currentUsername && x.RecipientUsername == recipientUsername && !x.SenderDeleted)
            )
            .OrderBy(x => x.MessageSent)
            .AsQueryable();

        var unreadMessages = query
            .Where(x => x.DateRead == null && x.RecipientUsername == currentUsername)
            .ToList();

        if (unreadMessages.Any())
        {
            unreadMessages.ForEach(m => m.DateRead = DateTime.UtcNow);
        }

        return await query.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<Group?> GetMessageGroup(string groupName)
    {
        return await context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
    }

    public void RemoveConnection(Connection connection)
    {
        context.Connections.Remove(connection);
    }

  
}