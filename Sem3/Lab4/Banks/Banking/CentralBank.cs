using Banks.Exceptions;
using Banks.Models;

namespace Banks.Banking;

public class CentralBank
{
    private List<Bank> banks = new List<Bank>();

    private CentralBank()
    {
    }

    public static CentralBank Shared { get; } = new CentralBank();
    public DateTime Time { get; private set; } = DateTime.Now;
    public IReadOnlyList<Bank> Banks => banks;

    public void ChangeTime(DateTime newTime)
    {
        if (newTime < Time)
            throw new BanksNegativeValueException("New date can't be earlier than the previous");
        lock (this)
        {
            var days = (newTime.Date - Time.Date).Days;
            if (days == 0)
                return;
            banks.ForEach(b => b.AdvanceTime(days));
        }
    }

    public Bank RegisterBank(string name, BankSettings settings)
    {
        if (banks.Any(b => b.Name == name))
            throw new BanksDuplicateException($"Bank with name {name} already exists");
        var bank = new Bank(name, settings);
        banks.Add(bank);
        return bank;
    }

    public TransferTransaction InitiateTransfer(Account from, Account to, decimal amount) =>
        new TransferTransaction(from, to, amount);
}