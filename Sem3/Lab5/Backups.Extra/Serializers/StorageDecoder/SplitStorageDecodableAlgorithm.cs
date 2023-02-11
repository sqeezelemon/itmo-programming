using Backups.Storages;
using Backups.Utilities;

namespace Backups.Extra.Serializers;

public class SplitStorageDecodableAlgorithm : IDecodableStorageAlgorithm
{
    public void Decode(Storage storage, IRepository repository, Dictionary<string, string> locations, IDecoder decoder)
    {
       foreach (var obj in storage.Location.Contents)
       {
           if (obj is not IFile archivedFile)
                continue;
           if (!locations.ContainsKey(Path.GetFileName(archivedFile.Path) !))
               continue;
           var locationStr = locations[Path.GetFileName(archivedFile.Path)];
           var location = repository.GetObject(locationStr);
           if (location is not IFolder destination)
                continue;
           decoder.Decode(archivedFile, destination, repository);
       }
    }
}