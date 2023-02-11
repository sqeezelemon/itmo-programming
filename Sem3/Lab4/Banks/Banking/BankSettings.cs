using Banks.Exceptions;
using Banks.Utils;
namespace Banks.Banking;

public class BankSettings
{
    public BankSettings(
        decimal creditComission,
        decimal debitCashback,
        decimal suspiciousTransferLimit,
        decimal suspiciousWithdrawLimit,
        Func<decimal, decimal> depositPercent)
    {
        BanksNegativeValueException.ThrowIfNegative(creditComission);
        BanksNegativeValueException.ThrowIfNegative(debitCashback);
        BanksNegativeValueException.ThrowIfNegative(suspiciousTransferLimit);
        BanksNegativeValueException.ThrowIfNegative(suspiciousWithdrawLimit);
        ArgumentNullException.ThrowIfNull(depositPercent);

        (CreditComission, DebitCashback, SuspiciousTransferLimit, SuspiciousWithdrawLimit, DepositPercent) = (creditComission, debitCashback, suspiciousTransferLimit, suspiciousWithdrawLimit, depositPercent);
    }

    public decimal CreditComission { get; private set; }
    public decimal DebitCashback { get; private set; }
    public Func<decimal, decimal> DepositPercent { get; private set; }
    public decimal SuspiciousTransferLimit { get; private set; }
    public decimal SuspiciousWithdrawLimit { get; private set; }
    internal Bank AssociatedBank { get; private set; } = null!;

    public void SetCreditComission(decimal value)
    {
        BanksNegativeValueException.ThrowIfNegative(value);
        AssociatedBank.HandleUpdate(
            "credit.tarif.comission",
            $"Credit comission changed from {CreditComission} to {value}");
        CreditComission = value;
    }

    public void SetDebitCashback(decimal value)
    {
        BanksNegativeValueException.ThrowIfNegative(value);
        AssociatedBank.HandleUpdate(
            "debit.tarif.cashback",
            $"Debit cashback changed from {DebitCashback} to {value}");
        DebitCashback = value;
    }

    public void SetDepositPercent(Func<decimal, decimal> value)
    {
        ArgumentNullException.ThrowIfNull(value);
        AssociatedBank.HandleUpdate(
            "deposit.tarif.percent",
            "Deposit rates were changed, see tarif details");
        DepositPercent = value;
    }

    public void SetSuspiciousTransferLimit(decimal value)
    {
        BanksNegativeValueException.ThrowIfNegative(value);
        AssociatedBank.HandleUpdate(
            "suspicious.transfer_limit",
            $"Transfer limit for suspicious clients changed from {SuspiciousTransferLimit} to {value}");
        SuspiciousTransferLimit = value;
    }

    public void SetSuspiciousWithdrawLimit(decimal value)
    {
        BanksNegativeValueException.ThrowIfNegative(value);
        AssociatedBank.HandleUpdate(
            "suspicious.transfer_limit",
            $"Withdraw limit for suspicious clients changed from {SuspiciousWithdrawLimit} to {value}");
        SuspiciousWithdrawLimit = value;
    }

    public BankSettings Copy()
    {
        var copy = (BankSettings)MemberwiseClone();
        copy.AssociatedBank = null!;
        return copy;
    }

    internal void SetAssociatedBank(Bank bank)
    {
        ArgumentNullException.ThrowIfNull(bank);
        if (AssociatedBank != null)
            throw new BanksSubscriptionException("These settings are already used by another bank");
        AssociatedBank = bank;
    }
}