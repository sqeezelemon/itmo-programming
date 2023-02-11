using Backups.Exceptions;
using Backups.Storages;
using Backups.Utilities;
namespace Backups.Backups;

public class BackupTask : IBackupTask
{
    private List<IBackupObject> _backupObjects = new List<IBackupObject>();
    public BackupTask(
        IRepository repository,
        IFolder backupFolder,
        IEncoder encoder,
        IStorageAlgorithm storageAlgorithm,
        IBackup backup)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(backupFolder);
        ArgumentNullException.ThrowIfNull(encoder);
        ArgumentNullException.ThrowIfNull(storageAlgorithm);
        ArgumentNullException.ThrowIfNull(backup);
        (Repository, BackupFolder, Encoder, StorageAlgorithm, Backup) = (repository, backupFolder, encoder, storageAlgorithm, backup);
    }

    public IRepository Repository { get; }
    public IFolder BackupFolder { get; }
    public IEncoder Encoder { get; }
    public IStorageAlgorithm StorageAlgorithm { get; }
    public IBackup Backup { get; }

    public void AddBackupObject(IRepoObject obj, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(obj);
        if (name is null)
        {
            name = Path.GetFileName(obj.Path);
            if (string.IsNullOrWhiteSpace(name))
                name = Path.GetDirectoryName(obj.Path);
            if (string.IsNullOrWhiteSpace(name))
                throw new BackupsInvalidPathException("Can't infer file name from context");
        }

        if (ContainsRepoObject(obj))
            throw new BackupsDuplicateException("Object is already included");

        if (ContainsBackupObject(name!))
            throw new BackupsDuplicateException($"Backup object with name {name!} already exists");

        var backObj = new BackupObject(name!, obj);
        _backupObjects.Add(backObj);
    }

    public void RemoveBackupObject(string name)
    {
        if (_backupObjects.RemoveAll(bo => bo.Name == name) < 0)
            throw new BackupsNotFoundException($"Backup Object with name {name} not found");
    }

    public virtual RestorePoint Perform(string name)
    {
        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
            throw new BackupsInvalidPathException($"Name {name} contains illegal characters.");

        var resPointFolder = Repository.MakeFolder(Path.Combine(BackupFolder.Path, name));
        Storage storage = StorageAlgorithm.Encode(_backupObjects, Repository, resPointFolder, Encoder);
        RestorePoint resPoint = new RestorePoint(DateTime.Now, name, storage, _backupObjects);
        Backup.AddRestorePoint(resPoint);
        return resPoint;
    }

    public bool ContainsRepoObject(IRepoObject obj) => _backupObjects.Any(bo => bo.Value.Path.StartsWith(obj.Path));
    public bool ContainsBackupObject(string name) => _backupObjects.Any(bo => bo.Name == name);
}