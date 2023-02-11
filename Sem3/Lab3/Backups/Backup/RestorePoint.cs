using Backups.Exceptions;
using Backups.Storages;
using Backups.Utilities;
namespace Backups.Backups;

public class RestorePoint : IRestorePoint
{
    private List<IBackupObject> _backupObjects;

    public RestorePoint(DateTime date, string name, Storage storage, IReadOnlyList<IBackupObject> backupObjects)
    {
        ArgumentNullException.ThrowIfNull(date);
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(backupObjects);

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
            throw new BackupsInvalidPathException($"Name {name} contains illegal characters.");
        (DateTime, Name, Storage) = (date, name, storage);
        _backupObjects = backupObjects.Select(bo => bo.GetCopy()).ToList();
    }

    public DateTime DateTime { get; internal set; }
    public string Name { get; }
    public IReadOnlyList<IBackupObject> BackupObjects => _backupObjects;
    public Storage Storage { get; }
    public IFolder Location => Storage.Location;

    public void ChangeDate(DateTime newDate)
    {
        ArgumentNullException.ThrowIfNull(newDate);
        DateTime = newDate;
    }
}