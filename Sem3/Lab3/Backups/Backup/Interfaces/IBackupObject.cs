using Backups.Utilities;

namespace Backups.Backups;

public interface IBackupObject
{
    string Name { get; }
    IRepoObject Value { get; }
    IBackupObject GetCopy();
}