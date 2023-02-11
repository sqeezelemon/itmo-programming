using System;
using Banks.Banking;
using Banks.Exceptions;

namespace Banks.Models;

public class CreditAccount : Account
{
    private const string NotificationType = "credit";

    internal CreditAccount(Bank bank, Client client)
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
        Balance -= amount;
    }

    protected override decimal BalanceDiff(int days) => (Balance >= 0) ? 0 : (Balance * Bank.Settings.CreditComission * days);

    protected override bool IsRelevant(string notificationType) => notificationType.StartsWith(NotificationType);
}