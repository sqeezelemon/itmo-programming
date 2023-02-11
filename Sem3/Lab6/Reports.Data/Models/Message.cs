using Reports.Data.Enums;

namespace Reports.Models;

public class Message
{
    public Message(Guid id, string serviceId, Account sender, Account recipient, DateTime timeStamp, string value, MessageState state)
    {
        (Id, ServiceId, Sender, Recipient, TimeStamp, Value, State) = (id, serviceId, sender, recipient, timeStamp, value, state);
    }

    protected Message() { }

    public Guid Id { get; set; }
    public string Value { get; set; }
    public string ServiceId { get; set; }
    public virtual Account Sender { get; set; }
    public virtual Account Recipient { get; set; }
    public DateTime TimeStamp { get; set; }
    public MessageState State { get; set; }
}