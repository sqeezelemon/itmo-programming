using Backups.Backups;
using Backups.Extra.Backups;
using Backups.Storages;
using Backups.Utilities;

namespace Backups.Extra.Algorithms;

public class SingleStorageMergeAlgorithm : MergeAlgorithm
{
    public SingleStorageMergeAlgorithm()
    {
        AddAllowedAlgo(typeof(SingleStorageAlgorithm));
    }

    public override IRestorePoint Merge(IRestorePoint x, IRestorePoint y, IFolder backupsFolder, IRepository repository, string? name = null)
    {
        IRestorePoint latest = (x.DateTime > y.DateTime) ? x : y;
        var rpName = name == null ? DefaultName(x, y) : name!;
        var rpFolder = repository.MakeFolder(Path.Combine(backupsFolder.Path, rpName));
        var copyStream = new MemoryStream();
        var latestArchive = (repository.GetObject(Path.Combine(y.Location.Path, "archive")) as IFile) !;
        latestArchive.Read(copyStream);
        var newArchive = repository.MakeFile(Path.Combine(rpFolder.Path, "archive"));
        copyStream.Seek(0, SeekOrigin.Begin);
        newArchive.Write(copyStream);

        var result = new RestorePointExtra(new RestorePoint(
            DateTime.Now,
            name == null ? DefaultName(x, y) : name!,
            new Storage(repository, rpFolder),
            latest.BackupObjects));
        return result;
    }
}