using Banks.Exceptions;
using Banks.Models;
using Banks.Utils;
namespace Banks.Banking;

public class Bank : IObserver
{
    private List<Account> accounts = new List<Account>();
    private List<Client> clients = new List<Client>();

    internal Bank(string name, BankSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (string.IsNullOrWhiteSpace(name))
            throw new BanksInvalidValueException("Bank name can't be empty");
        (Name, Settings) = (name, settings.Copy());
        Settings.SetAssociatedBank(this);
    }

    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; private set; }
    public BankSettings Settings { get; }
    public DateTime Time => CentralBank.Shared.Time;

    public IReadOnlyList<Account> Accounts => accounts;
    public IReadOnlyList<Client> Clients => clients;

    public void AcceptClient(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);
        if (HasClient(client))
            throw new BanksDuplicateException("Client is already registered");
        clients.Add(client);
    }

    public DebitAccount MakeDebitAccount(Client client)
    {
        ValidateClient(client);
        var account = new DebitAccount(this, client);
        accounts.Add(account);
        return account;
    }

    public CreditAccount MakeCreditAccount(Client client)
    {
        ValidateClient(client);
        var account = new CreditAccount(this, client);
        accounts.Add(account);
        return account;
    }

    public DepositAccount MakeDepositAccount(Client client, DateTime endDate)
    {
        ValidateClient(client);
        ArgumentNullException.ThrowIfNull(endDate);
        var account = new DepositAccount(this, client, endDate);
        accounts.Add(account);
        return account;
    }

    public void HandleUpdate(string type, string value) => accounts.ForEach(a => a.HandleUpdate(type, value));

    internal void AdvanceTime(int days) => accounts.ForEach(a => a.AdvanceTime(days));

    private void ValidateClient(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);
        if (!HasClient(client))
            throw new BanksNotFoundException("Client not found");
    }

    private bool HasClient(Client client) => clients.Any(c => c.PhoneNumber == client.PhoneNumber);
}