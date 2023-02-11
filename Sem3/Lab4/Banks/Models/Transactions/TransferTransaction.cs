using System.Security.Principal;
using Banks.Exceptions;
namespace Banks.Models;

public class TransferTransaction : Transaction
{
    internal TransferTransaction(Account sender, Account recepient, decimal amount)
        : base(amount)
    {
        BanksNegativeValueException.ThrowIfNegative(amount);
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(recepient);
        (Sender, Recepient) = (sender, recepient);
    }

    public Account Sender { get; }
    public Account Recepient { get; }

    public override void Execute()
    {
        lock (this)
        {
            ValidateExecution();
            Sender.ValidateTransfer(Amount);
            Sender.Decrease(Amount);
            Recepient.Add(Amount);

            IsExecuted = true;
            Sender.AddTransaction(this);
            Recepient.AddTransaction(this);
        }
    }

    public override void Cancel()
    {
        lock (this)
        {
            ValidateCancelation();
            Recepient.Decrease(Amount, false);
            Sender.Add(Amount);
            IsCanceled = true;

            var senderCancelation = new CanceledTransaction(Sender, this, Amount);
            var recepientCancelation = new CanceledTransaction(Recepient, this, -Amount);
            Sender.AddTransaction(senderCancelation);
            Recepient.AddTransaction(recepientCancelation);
        }
    }

    public override bool InvolvesAccount(Account account) => Sender.Equals(account) || Recepient.Equals(account);
}