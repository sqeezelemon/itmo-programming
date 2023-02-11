using Backups.Storages;
using Backups.Utilities;

namespace Backups.Backups;

public interface IBackupTask
{
    IRepository Repository { get; }
    IFolder BackupFolder { get; }
    IEncoder Encoder { get; }
    IStorageAlgorithm StorageAlgorithm { get; }
    IBackup Backup { get; }

    void AddBackupObject(IRepoObject obj, string? name = null);
    void RemoveBackupObject(string name);
    RestorePoint Perform(string name);
    bool ContainsRepoObject(IRepoObject obj);
    bool ContainsBackupObject(string name);
}