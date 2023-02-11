using System;
using Banks.Banking;
using Banks.Exceptions;

namespace Banks.Models;

public class DepositAccount : Account
{
    private const string NotificationType = "deposit";

    internal DepositAccount(Bank bank, Client client, DateTime endDate)
        : base(bank, client)
    {
        if (endDate < CentralBank.Shared.Time)
            throw new BanksInvalidValueException("Deposit end date can't be in the past");
        EndDate = endDate;
    }

    public DateTime EndDate { get; }

    internal override void Add(decimal amount)
    {
        BanksNegativeValueException.ThrowIfNegative(amount);
        Balance += amount;
    }

    internal override void Decrease(decimal amount, bool validateBallance = true)
    {
        if (EndDate > CentralBank.Shared.Time)
            throw new BanksRejectionException("Can't withdraw from a non-expired deposit.");

        BanksNegativeValueException.ThrowIfNegative(amount);
        if (amount > Balance)
            throw new BanksRejectionException($"Trying to withdraw {amount} from an account that has {Balance}");
        Balance -= amount;
    }

    protected override decimal BalanceDiff(int days) => (Balance < 0) ? 0 : Balance * Bank.Settings.DepositPercent(Balance) * days;

    protected override bool IsRelevant(string notificationType) => notificationType.StartsWith(NotificationType);
}