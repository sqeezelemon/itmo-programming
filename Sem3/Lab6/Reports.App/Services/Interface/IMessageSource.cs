using Reports.App.Dto;
using Reports.Models;

namespace Reports.App.Services;

public interface IMessageSource
{
    void SendMessage(MessageDto message);
    IReadOnlyList<MessageDto> ReceiveMessages();
}