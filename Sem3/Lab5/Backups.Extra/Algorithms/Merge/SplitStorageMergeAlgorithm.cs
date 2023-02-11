using Backups.Backups;
using Backups.Extra.Backups;
using Backups.Storages;
using Backups.Utilities;

namespace Backups.Extra.Algorithms;

public class SplitStorageMergeAlgorithm : MergeAlgorithm
{
    public SplitStorageMergeAlgorithm()
    {
        AddAllowedAlgo(typeof(SplitStorageAlgorithm));
    }

    public override IRestorePoint Merge(IRestorePoint x, IRestorePoint y, IFolder backupsFolder, IRepository repository, string? name = null)
    {
        IRestorePoint previous = (x.DateTime <= y.DateTime) ? x : y;
        IRestorePoint latest = (x.DateTime > y.DateTime) ? x : y;
        var rpName = name == null ? DefaultName(x, y) : name!;
        var rpFolder = repository.MakeFolder(Path.Combine(backupsFolder.Path, rpName));
        var allBackupObjects = y.BackupObjects.ToList();
        allBackupObjects.AddRange(x.BackupObjects);
        var backupObjects = allBackupObjects
            .GroupBy(bo => bo.Name)
            .Select(g => g.First())
            .ToList();
        foreach (var bo in backupObjects)
        {
            var sourcePath = string.Empty;
            if (latest.BackupObjects.Contains(bo))
                sourcePath = Path.Combine(latest.Location.Path, bo.Name);
            else
                sourcePath = Path.Combine(previous.Location.Path, bo.Name);
            var sourceFile = (repository.GetObject(sourcePath) as IFile) !;
            var copyStream = new MemoryStream();
            sourceFile.Read(copyStream);
            copyStream.Position = 0;
            var newFile = repository.MakeFile(Path.Combine(rpFolder.Path, bo.Name));
            newFile.Write(copyStream);
        }

        var result = new RestorePointExtra(new RestorePoint(
            DateTime.Now,
            rpName,
            new Storage(repository, rpFolder),
            backupObjects));
        return result;
    }
}