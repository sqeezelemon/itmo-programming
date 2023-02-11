using Banks.Exceptions;
namespace Banks.Models;

public abstract class Transaction
{
    protected Transaction(decimal amount)
    {
        Amount = amount;
    }

    public decimal Amount { get; }
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsCanceled { get; protected set; } = false;
    public bool IsExecuted { get; protected set; } = false;

    public abstract void Execute();
    public abstract void Cancel();
    public abstract bool InvolvesAccount(Account account);

    protected void ValidateCancelation()
    {
        if (!IsExecuted)
            throw new BanksRejectionException("Can't cancel transaction that wasn't executed");
        if (IsCanceled)
            throw new BanksRejectionException("Can't cancel previously canceled transaction");
    }

    protected void ValidateExecution()
    {
        if (IsExecuted)
            throw new BanksRejectionException("Can't execute previously executed transaction");
        if (IsCanceled)
            throw new BanksRejectionException("Can't execute previously canceled transaction");
    }
}