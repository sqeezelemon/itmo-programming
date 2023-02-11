using Reports.Data.Enums;

namespace Reports.Models;

public class EmployeeAction
{
    public EmployeeAction(Guid id, DateTime timestamp, Account account, EmployeeActionType type)
    {
        ArgumentNullException.ThrowIfNull(account);
        (Id, Timestamp, Account, Type) = (id, timestamp, account, type);
    }

    protected EmployeeAction() { }

    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public virtual Account Account { get; set; }
    public EmployeeActionType Type { get; set; }
}