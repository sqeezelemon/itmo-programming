using Banks.Exceptions;

namespace Banks.Models;

public class CanceledTransaction : Transaction
{
    internal CanceledTransaction(Account account, Transaction associated, decimal amount)
        : base(amount)
    {
        BanksNegativeValueException.ThrowIfNegative(amount);
        ArgumentNullException.ThrowIfNull(associated);
        ArgumentNullException.ThrowIfNull(account);
        Account = account;
        Associated = associated;
        IsExecuted = true;
    }

    public Account Account { get; }
    public Transaction Associated { get; }

    public override void Execute()
    {
        // Already executed, acts more like metadata at this stage
    }

    public override void Cancel()
    {
        lock (this)
        {
            if (Amount < 0)
            {
                Account.Add(-Amount);
            }
            else
            {
                Account.Decrease(Amount, false);
            }

            var cancelation = new CanceledTransaction(Account, this, -Amount);
            Account.AddTransaction(cancelation);
            IsCanceled = true;
        }
    }

    public override bool InvolvesAccount(Account account) => Associated.InvolvesAccount(account);
}