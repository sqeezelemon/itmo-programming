using Backups.Backups;
using Backups.Exceptions;
using Backups.Extra.Utils;
using Backups.Utilities;

namespace Backups.Extra.Algorithms;

public class CleanupTask
{
    private List<ICleanupRule> rules = new List<ICleanupRule>();

    // >0 - Limit Implemented
    // =0 - Remove all
    // <0 - Disabled
    public int Threshold { get; set; } = -1;
    public Logger? Logger { get; set; }

    public void AddRule(ICleanupRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        if (rules.Contains(rule))
            throw new BackupsDuplicateException("Rule is already added");
        rules.Add(rule);
    }

    public void RemoveRule(ICleanupRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        if (!rules.Remove(rule))
            throw new BackupsNotFoundException("Rule didn't exist in this cleanup task");
    }

    public void Perform(IBackup backup, IRepository repository)
    {
        if (Threshold < 0)
            return;
        if (Threshold > rules.Count)
        {
            Logger?.Warning($"Threshold ({Threshold}) > Rule count ({rules.Count}) - ShouldDelete always returns false");
            return;
        }

        var restorePoints = backup.RestorePoints.OrderBy(rp => rp.DateTime).ToList();

        // -1 because we can't delete the last one
        for (int i = 0; i < restorePoints.Count - 1; i++)
        {
            if (!ShouldDelete(restorePoints[i], i, restorePoints.Count))
                continue;
            backup.RemoveRestorePoint(restorePoints[i].Name);
            repository.Delete(restorePoints[i].Location.Path);
            Logger?.Info($"Removed backup item {restorePoints[i].Name}");
        }
    }

    private bool ShouldDelete(IRestorePoint restorePoint, int index, int total)
    {
        var res = rules.Sum(r => r.ShouldDelete(restorePoint, index, total) ? 1 : 0);
        return res >= Threshold;
    }
}