using System;
using Backups.Backups;
using Backups.Utilities;

namespace Backups.Storages;

public class SplitStorageAlgorithm : IStorageAlgorithm
{
    public Storage Encode(List<IBackupObject> backupObjects, IRepository repository, IFolder outputFolder, IEncoder encoder)
    {
        foreach (var backObj in backupObjects)
        {
            var file = repository.MakeFile(Path.Combine(outputFolder.Path, backObj.Name));
            var stream = new MemoryStream();
            encoder.Encode(backObj.Value, stream);
            stream.Seek(0, SeekOrigin.Begin);
            file.Write(stream);
        }

        return new Storage(repository, outputFolder);
    }
}