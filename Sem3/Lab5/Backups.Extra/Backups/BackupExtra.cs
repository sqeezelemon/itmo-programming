using Backups.Backups;

namespace Backups.Extra.Backups;

public class BackupExtra : IBackup
{
    public BackupExtra(IBackup inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        Inner = inner;
    }

    public BackupExtra()
        : this(new Backup())
    {
    }

    public IReadOnlyList<IRestorePoint> RestorePoints => Inner.RestorePoints;

    private IBackup Inner { get; }

    public void AddRestorePoint(IRestorePoint restorePoint)
    {
        ArgumentNullException.ThrowIfNull(restorePoint);
        Inner.AddRestorePoint(restorePoint);
    }

    public void RemoveRestorePoint(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Can't infer restore point name from empty string");
        Inner.RemoveRestorePoint(name);
    }

    public bool HasRestorePoint(string name) => Inner.HasRestorePoint(name);
}