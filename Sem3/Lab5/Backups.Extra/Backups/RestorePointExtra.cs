using Backups.Backups;
using Backups.Extra.Utils;
using Backups.Storages;
using Backups.Utilities;

namespace Backups.Extra.Backups;

public class RestorePointExtra : IRestorePoint, ILoggable
{
    public RestorePointExtra(RestorePoint inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        Inner = inner;
    }

    public DateTime DateTime => Inner.DateTime;
    public string Name => Inner.Name;
    public IReadOnlyList<IBackupObject> BackupObjects => Inner.BackupObjects;
    public Storage Storage => Inner.Storage;
    public IFolder Location => Inner.Location;

    private RestorePoint Inner { get; }

    public void ChangeDate(DateTime newDate) => Inner.ChangeDate(newDate);

    public string LogString() => $"RestorePoint \"{Name}\" with {BackupObjects.Count} objects, created on {DateTime} @ {Location.Path}";
}