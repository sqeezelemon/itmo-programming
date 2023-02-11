using Backups.Backups;

namespace Backups.Extra.Algorithms;

public class DateCleanupRule : ICleanupRule
{
    public DateCleanupRule(DateTime min)
    {
        ArgumentNullException.ThrowIfNull(min);
        Min = min;
    }

    public DateTime Min { get; }

    public bool ShouldDelete(IRestorePoint restorePoint, int index, int total)
    {
        ArgumentNullException.ThrowIfNull(restorePoint);
        return restorePoint.DateTime < Min;
    }
}