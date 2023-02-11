using Microsoft.EntityFrameworkCore;
using Reports.App.Dto;
using Reports.App.Exceptions;
using Reports.App.Mapping;
using Reports.Data;
using Reports.Data.Enums;
using Reports.Models;

namespace Reports.App.Services;

public class MessageService : IMessageService
{
    private static Dictionary<string, IMessageSource> sources = new Dictionary<string, IMessageSource>();

    private AppDbContext _context;
    private IAccountService accountService;

    public MessageService(AppDbContext context, IAccountService service)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(service);
        (_context, accountService) = (context, service);
    }

    public async Task<MessageDto> SendMessage(string serviceId, string senderId, string recipientId, string value)
    {
        Account sender = await _context.Accounts.SingleOrDefaultAsync(a => a.Name == senderId && a.ServiceId == serviceId);
        if (sender is null)
            throw new ReportsNotFoundException($"Account with id {senderId} for service {serviceId} doesn't exist");
        Account recipient = await _context.Accounts.SingleOrDefaultAsync(a => a.Name == recipientId && a.ServiceId == serviceId);
        if (recipient is null)
            throw new ReportsNotFoundException($"Account with id {senderId} for service {serviceId} doesn't exist");
        IMessageSource source = sources[serviceId];
        if (serviceId is null)
            throw new ReportsNotFoundException($"Service with id {serviceId} not found");
        var message = new Message(Guid.NewGuid(), serviceId, sender, recipient, DateTime.Now, value, MessageState.Handled);
        source.SendMessage(message.AsDto());
        _context.Add(message);
        await _context.SaveChangesAsync();
        return message.AsDto();
    }

    public async Task<IReadOnlyList<MessageDto>> GetUnhandledMessages(string employeeLogin)
    {
        return await _context.Messages
            .Where(m => m.State != MessageState.Handled && m.State != MessageState.Outbound && m.Recipient.Owner != null && m.Recipient.Owner.Login == employeeLogin)
            .Select(m => m.AsDto())
            .ToListAsync();
    }

    // Checks message sources for new messages
    public async Task<IReadOnlyList<MessageDto>> FetchNewMessages()
    {
        var messages = new List<Message>();
        foreach (var dto in sources.SelectMany(s => s.Value.ReceiveMessages()))
        {
            if (await accountService.FindAccount(dto.senderId, dto.serviceId) == null)
                await accountService.MakeAccount(dto.senderId, dto.serviceId, null);
            Account sender = _context.Accounts.Single(a => a.Name == dto.senderId && a.ServiceId == dto.serviceId);

            // Staff account should exist
            await accountService.FindAccount(dto.recepientId, dto.serviceId);
            Account recipient =
                _context.Accounts.First(a => a.Name == dto.recepientId && a.ServiceId == dto.serviceId) ?? throw new ReportsNotFoundException("Employee account not found");

            var message = new Message(
                dto.id,
                dto.serviceId,
                sender,
                recipient,
                dto.timestamp,
                dto.value,
                MessageState.Received);
            messages.Add(message);
        }

        await _context.Messages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();
        return messages.Select(m => m.AsDto()).ToList();
    }

    // Note: To handle messages, use HandleMessage
    public async Task<MessageDto> ChangeState(Guid id, MessageState newState)
    {
        var message = await _context.Messages.SingleOrDefaultAsync(m => m.Id == id);
        if (message == null)
            throw new ReportsNotFoundException($"Message with id {id} not found");
        message.State = newState;
        await _context.SaveChangesAsync();
        return message.AsDto();
    }

    public async Task<MessageDto> HandleMessage(Guid id, string handlerLogin)
    {
        var message = await _context.Messages.SingleOrDefaultAsync(m => m.Id == id);
        if (message == null)
            throw new ReportsNotFoundException($"Message with id {id} not found");
        await accountService.FindAccount(handlerLogin, message.ServiceId);
        Account handler = _context.Accounts.Single(a => a.Name == handlerLogin && a.ServiceId == message.ServiceId);

        message.State = MessageState.Handled;
        var action = new EmployeeAction(Guid.NewGuid(), DateTime.Now, handler, EmployeeActionType.HandledMessage);
        await _context.Actions.AddAsync(action);
        await _context.SaveChangesAsync();
        return message.AsDto();
    }

    public void AddMessageSource(string name, IMessageSource source)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (sources.ContainsKey(name))
            throw new ReportsDuplicateException($"Message source key {name} is already in use.");
        sources[name] = source;
    }

    public void RemoveMessageSource(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!sources.Remove(name))
            throw new ReportsNotFoundException($"Source with name {name} not found");
    }
}