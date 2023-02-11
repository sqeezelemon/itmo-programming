using Backups.Storages;
using Backups.Utilities;

namespace Backups.Extra.Serializers;

public class SingleStorageDecodableAlgorithm : SingleStorageAlgorithm, IDecodableStorageAlgorithm
{
    public void Decode(Storage storage, IRepository repository, Dictionary<string, string> locations, IDecoder decoder)
    {
        var tmpFolder = repository.MakeFolder(storage.Location.Path + Path.DirectorySeparatorChar + ".tmp");
        var singleFile = repository.GetObject(storage.Location.Path + Path.DirectorySeparatorChar + "archive") as IFile;
        decoder.Decode(singleFile!, tmpFolder, repository);
        foreach (var obj in tmpFolder.Contents)
        {
            if (obj is not IFile archivedFile)
                continue;
            var locationStr = locations[Path.GetFileName(archivedFile.Path)];
            var location = repository.GetObject(locationStr);
            if (location is not IFolder destination)
                continue;
            decoder.Decode(archivedFile, destination, repository);
        }

        repository.Delete(tmpFolder.Path);
    }
}