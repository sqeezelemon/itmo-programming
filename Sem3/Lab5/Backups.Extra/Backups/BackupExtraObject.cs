using Backups.Backups;
using Backups.Extra.Utils;
using Backups.Utilities;

namespace Backups.Extra.Backups;

public class BackupExtraObject : IBackupObject, ILoggable
{
    public BackupExtraObject(BackupObject inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        Inner = inner;
    }

    public string Name => Inner.Name;
    public IRepoObject Value => Inner.Value;

    private BackupObject Inner { get; }

    public IBackupObject GetCopy() => (MemberwiseClone() as BackupExtraObject) !;

    public string LogString() => $"BackupObject \"{Name}\" @ {Value.Path}";
}