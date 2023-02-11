using System;
using Backups.Exceptions;

namespace Backups.Backups;

public class Backup : IBackup
{
    private List<IRestorePoint> _restorePoints = new List<IRestorePoint>();

    public Backup()
    {
    }

    public IReadOnlyList<IRestorePoint> RestorePoints => _restorePoints;

    public void AddRestorePoint(IRestorePoint restorePoint)
    {
        if (HasRestorePoint(restorePoint.Name))
            throw new BackupsDuplicateException("Restore point is already in backup");
        _restorePoints.Add(restorePoint);
    }

    public void RemoveRestorePoint(string name)
    {
        if (_restorePoints.RemoveAll(r => r.Name == name) == 0)
            throw new BackupsNotFoundException($"Restore point with name {name} not found");
    }

    public bool HasRestorePoint(string name) => _restorePoints.Any(rp => rp.Name == name);
}