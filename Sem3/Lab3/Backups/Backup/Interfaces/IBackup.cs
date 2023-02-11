namespace Backups.Backups;

public interface IBackup
{
    IReadOnlyList<IRestorePoint> RestorePoints { get; }

    void AddRestorePoint(IRestorePoint restorePoint);

    void RemoveRestorePoint(string name);

    bool HasRestorePoint(string name);
}