using Backups.Storages;
using Backups.Utilities;

namespace Backups.Backups;

public interface IRestorePoint
{
    DateTime DateTime { get; }
    string Name { get; }
    IReadOnlyList<IBackupObject> BackupObjects { get; }
    Storage Storage { get; }
    IFolder Location { get; }

    void ChangeDate(DateTime newDate);
}