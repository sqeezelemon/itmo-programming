using Reports.App.Dto;
using Reports.App.Services;
using Reports.Data.Enums;
using Reports.Models;

namespace Reports.Test;

public class MessageGenerator : IMessageSource
{
    private string recipient;

    public MessageGenerator(string recipientLogin)
    {
        recipient = recipientLogin;
    }

    public void SendMessage(MessageDto message)
    {
        return;
    }

    public IReadOnlyList<MessageDto> ReceiveMessages()
    {
        return new List<MessageDto>()
        {
            new MessageDto(Guid.NewGuid(), "test", "sender1", recipient, "Lorem Ipsum", DateTime.Now, MessageState.New),
            new MessageDto(Guid.NewGuid(), "test", "sender2", recipient, "Sin Dolor Amet", DateTime.Now, MessageState.New),
        };
    }
}