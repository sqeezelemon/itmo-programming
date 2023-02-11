using Reports.App.Dto;
using Reports.Data.Enums;

namespace Reports.App.Services;

public interface IMessageService
{
    Task<MessageDto> SendMessage(string serviceId, string senderId, string recipientId, string value);
    Task<IReadOnlyList<MessageDto>> GetUnhandledMessages(string employeeLogin);
    Task<IReadOnlyList<MessageDto>> FetchNewMessages();
    Task<MessageDto> ChangeState(Guid id, MessageState newState);
    Task<MessageDto> HandleMessage(Guid id, string handlerLogin);
    void AddMessageSource(string name, IMessageSource source);
    void RemoveMessageSource(string name);
}