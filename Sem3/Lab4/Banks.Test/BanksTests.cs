using Banks.Banking;
using Banks.Exceptions;
using Banks.Models;
using Banks.Utils;
using Xunit;

namespace Banks.Test;

internal class DummyObserver : IObserver
{
    public DummyObserver(Action<string, string> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        Handler = handler;
    }

    public Action<string, string> Handler { get; }

    public void HandleUpdate(string type, string value) => Handler(type, value);
}

public class BanksTests
{
    private CentralBank CB => CentralBank.Shared;
    private BankSettings Settings { get; } = new BankSettings(0.01M, 0.01M, 50000M, 50000M, DepositPercent);

    [Fact]
    public void CentralBank_TimeRewindWorks()
    {
        Bank bank = CB.RegisterBank("Penkoff", Settings);
        Client client = new Client("Vasya Pupkin", "+79008007060");
        bank.AcceptClient(client);
        Account account = bank.MakeDebitAccount(client);
        account.InitiateAdd(50000M).Execute();
        var newTime = new DateTime(
            CB.Time.AddMonths(1).Year,
            CB.Time.AddMonths(1).Month,
            1);
        int expectedDays = (newTime - CB.Time.Date).Days - 1;
        decimal expected = (50000M * Settings.DebitCashback * expectedDays) + 50000M;

        CB.ChangeTime(newTime);

        Assert.Equal(expected, account.Balance);
    }

    [Fact]
    public void Transaction_CanExecuteAndCancel()
    {
        Bank bank = CB.RegisterBank("SberBanks.Extra", Settings);
        Client client = new Client("Vladislav Ziminsky", "+70123456789");
        bank.AcceptClient(client);
        Account account = bank.MakeDebitAccount(client);
        Transaction addTransaction = account.InitiateAdd(50000M);
        Transaction withdrawTransaction = account.InitiateWithdraw(10000M);

        Assert.Equal(0, account.Balance);

        addTransaction.Execute();
        Assert.Equal(50000M, account.Balance);

        withdrawTransaction.Execute();
        Assert.Equal(40000M, account.Balance);

        addTransaction.Cancel();
        Assert.Equal(-10000M, account.Balance);

        Assert.Throws<BanksRejectionException>(() => addTransaction.Cancel());
    }

    [Fact]
    public void Bank_TermsChangeNotificationsWork()
    {
        Bank bank = CB.RegisterBank("Beta bank", Settings);
        Client client = new Client("Binard Binarsky", "+10001110011", "1111000000");
        bank.AcceptClient(client);
        Account account = bank.MakeDebitAccount(client);
        bool wasNotified = false;
        var observer = new DummyObserver((_, _) => wasNotified = true);
        account.Subscribe(observer);

        bank.Settings.SetCreditComission(2M);
        Assert.False(wasNotified); // Should not be notified of credit changes

        bank.Settings.SetSuspiciousWithdrawLimit(50000M);
        Assert.False(wasNotified); // Client has a passport, so shouldn't be notified of changes for suspicious ones

        bank.Settings.SetDebitCashback(1M);
        Assert.True(wasNotified); // Should notify because we subscribed to a debit account

        wasNotified = false;
        client.RemovePassport();

        bank.Settings.SetSuspiciousWithdrawLimit(40000M);
        Assert.True(wasNotified); // Client's passport was removed, should notify because he's sus now
    }

    private static decimal DepositPercent(decimal value)
    {
        if (value <= 50000)
            return 0.03M;
        if (value <= 100000)
            return 0.04M;
        return 0.05M;
    }
}