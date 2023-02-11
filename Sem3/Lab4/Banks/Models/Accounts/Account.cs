using Banks.Banking;
using Banks.Exceptions;
using Banks.Utils;
namespace Banks.Models;

public abstract class Account : IObservable, IObserver
{
    private List<Transaction> transactions = new List<Transaction>();
    private List<IObserver> observers = new List<IObserver>();

    protected Account(Bank bank, Client client)
    {
        ArgumentNullException.ThrowIfNull(bank);
        ArgumentNullException.ThrowIfNull(client);
        (Bank, Client) = (bank, client);
    }

    public IReadOnlyList<Transaction> Transactions => transactions;
    public Guid Id { get; } = Guid.NewGuid();
    public Bank Bank { get; }
    public Client Client { get; }
    public decimal Balance { get; protected set; } = 0;
    protected decimal Pending { get; set; } = 0;

    public AddTransaction InitiateAdd(decimal amount) => new AddTransaction(this, amount);
    public WithdrawTransaction InitiateWithdraw(decimal amount) => new WithdrawTransaction(this, amount);

    public void HandleUpdate(string type, string value)
    {
        if (
            type.StartsWith("all")
            || (type.StartsWith("suspicious") && Client.IsSuspicious)
            || IsRelevant(type))
            NotifyAll(type, value);
    }

    public void Subscribe(IObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        if (observers.Contains(observer))
            throw new BanksDuplicateException("Observer is already subscribed");
        observers.Add(observer);
    }

    public void Unsubscribe(IObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        if (!observers.Remove(observer))
            throw new BanksNotFoundException("Observer wasn't subscribed to this account");
    }

    internal void AdvanceTime(int days)
    {
        int daysLeft = days;
        if (Bank.Time.AddDays(days).Month == Bank.Time.Month)
        {
            Pending += BalanceDiff(days);
            return;
        }

        var currDate = new DateTime(
            Bank.Time.Date.AddMonths(1).Year,
            Bank.Time.Date.AddMonths(1).Month,
            1);
        Pending += BalanceDiff((currDate - Bank.Time.Date).Days - 1);
        daysLeft -= (currDate - Bank.Time.Date).Days - 1;

        // Looping over every first of the month
        while (true)
        {
            Balance += Pending;
            Pending = 0;
            var oldDate = currDate;
            currDate = currDate.AddMonths(1);
            var daysInMonth = (currDate - oldDate).Days;

            if (daysInMonth >= daysLeft)
            {
                Pending += BalanceDiff(daysLeft);

                break;
            }

            Pending += BalanceDiff(daysInMonth);
            currDate.AddMonths(1);
            daysLeft -= daysInMonth;
        }
    }

    internal abstract void Add(decimal amount);
    internal abstract void Decrease(decimal amount, bool validateBallance = true);

    internal void AddTransaction(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        if (!transaction.InvolvesAccount(this))
            throw new BanksNotFoundException("Transaction is not associated with this account");
        if (transactions.Any(t => t.Id == transaction.Id))
            throw new BanksDuplicateException($"Transaction with ID {transaction.Id} already exists in this account");
        transactions.Add(transaction);
    }

    // Checks whether the transfer is allowed if the client is suspicious
    internal void ValidateTransfer(decimal amount)
    {
        if (Client.IsSuspicious && amount > Bank.Settings.SuspiciousTransferLimit)
            throw new BanksRejectionException($"Transfer amount ({amount}) is over the suspicious client limit ({Bank.Settings.SuspiciousTransferLimit} max)");
    }

    // Checks whether the withdrawal is allowed if the client is suspicious
    internal void ValidateWithdraw(decimal amount)
    {
        if (Client.IsSuspicious && amount > Bank.Settings.SuspiciousWithdrawLimit)
            throw new BanksRejectionException($"Withdraw amount ({amount}) is over the suspicious client limit ({Bank.Settings.SuspiciousWithdrawLimit} max)");
    }

    // How much the balance changes after x days, used for unified AdvanceTime
    protected abstract decimal BalanceDiff(int days);

    protected abstract bool IsRelevant(string notificationType);

    private void NotifyAll(string type, string value) => observers.ForEach(o => o.HandleUpdate(type, value));
}