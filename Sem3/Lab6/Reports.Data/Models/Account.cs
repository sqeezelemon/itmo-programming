namespace Reports.Models;

public class Account
{
    public Account(string id, string serviceId, Employee owner)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(serviceId);
        (Name, ServiceId, Owner) = (id, serviceId, owner);
    }

    protected Account() { }

    public Guid Id { get; set; } = Guid.NewGuid();

    // Id of the account inside the service
    public string Name { get; set; }

    // Id of the service to which the account belongs
    public string ServiceId { get; set; }
    public virtual Employee Owner { get; set; }
}