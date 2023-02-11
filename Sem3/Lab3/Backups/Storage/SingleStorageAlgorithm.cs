using System;
using Backups.Backups;
using Backups.Utilities;

namespace Backups.Storages;

public class SingleStorageAlgorithm : IStorageAlgorithm
{
    internal const string ArchiveName = "archive";
    internal const string TempDirName = "tmp";

    public Storage Encode(List<IBackupObject> backupObjects, IRepository repository, IFolder outputFolder, IEncoder encoder)
    {
        var tmpDir = repository.MakeFolder(Path.Combine(outputFolder.Path, TempDirName));
        foreach (var backObj in backupObjects)
        {
            var file = repository.MakeFile(Path.Combine(tmpDir.Path, backObj.Name));
            var stream = new MemoryStream();
            encoder.Encode(backObj.Value, stream);
            stream.Seek(0, SeekOrigin.Begin);
            file.Write(stream);
        }

        var archiveFile = repository.MakeFile(Path.Combine(outputFolder.Path, ArchiveName));
        var archiveStream = new MemoryStream();
        encoder.Encode(tmpDir, archiveStream);
        archiveStream.Seek(0, SeekOrigin.Begin);
        archiveFile.Write(archiveStream);
        repository.Delete(tmpDir.Path);

        return new Storage(repository, outputFolder);
    }
}