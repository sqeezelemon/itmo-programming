using Reports.App.Dto;
using Reports.Models;

namespace Reports.App.Mapping;

public static class MessageMapping
{
    public static MessageDto AsDto(this Message message)
    {
        return new MessageDto(
            message.Id,
            message.ServiceId,
            message.Sender.Name,
            message.Recipient.Name,
            message.Value,
            message.TimeStamp,
            message.State);
    }
}