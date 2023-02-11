using Banks.Banking;
using Banks.Exceptions;
namespace Banks.Models;

public class DebitAccount : Account
{
    private const string NotificationType = "debit";

    internal DebitAccount(Bank bank, Client client)
        : base(bank, client)
    {
    }

    internal override void Add(decimal amount)
    {
        BanksNegativeValueException.ThrowIfNegative(amount);
        Balance += amount;
    }

    internal override void Decrease(decimal amount, bool validateBallance = true)
    {
        BanksNegativeValueException.ThrowIfNegative(amount);
        if (amount > Balance && validateBallance)
            throw new BanksRejectionException($"Trying to withdraw {amount} from an account that has {Balance}");
        Balance -= amount;
    }

    protected override decimal BalanceDiff(int days) => (Balance < 0) ? 0 : Balance * Bank.Settings.DebitCashback * days;

    protected override bool IsRelevant(string notificationType) => notificationType.StartsWith(NotificationType);
}