using Banks.Exceptions;
namespace Banks.Models;

public class AddTransaction : Transaction
{
    internal AddTransaction(Account account, decimal amount)
        : base(amount)
    {
        BanksNegativeValueException.ThrowIfNegative(amount);
        ArgumentNullException.ThrowIfNull(account);
        Account = account;
    }

    public Account Account { get; }

    public override void Execute()
    {
        lock (this)
        {
            ValidateExecution();
            Account.Add(Amount);
            IsExecuted = true;
            Account.AddTransaction(this);
        }
    }

    public override void Cancel()
    {
        lock (this)
        {
            ValidateCancelation();
            Account.Decrease(Amount, false);
            IsCanceled = true;
            var cancelation = new CanceledTransaction(Account, this, Amount);
            Account.AddTransaction(cancelation);
        }
    }

    public override bool InvolvesAccount(Account account) => account.Equals(Account);
}