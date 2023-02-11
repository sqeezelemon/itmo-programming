using Backups.Backups;

namespace Backups.Extra.Algorithms;

public class IndexCleanupRule : ICleanupRule
{
    public IndexCleanupRule(int amount)
    {
        if (Amount < 0)
            throw new IndexOutOfRangeException("Amount can't be negative");
        Amount = amount;
    }

    public int Amount { get; }

    public bool ShouldDelete(IRestorePoint restorePoint, int index, int total) => total - index <= Amount;
}