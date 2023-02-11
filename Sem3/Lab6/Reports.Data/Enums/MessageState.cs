namespace Reports.Data.Enums;

public enum MessageState
{
    New,
    Received,
    Handled,
    Outbound, // For messages sent by employees
}