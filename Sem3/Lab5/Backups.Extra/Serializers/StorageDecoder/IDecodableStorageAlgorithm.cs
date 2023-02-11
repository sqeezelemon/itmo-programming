using Backups.Backups;
using Backups.Storages;
using Backups.Utilities;

namespace Backups.Extra.Serializers;

public interface IDecodableStorageAlgorithm
{
    void Decode(Storage storage, IRepository repository, Dictionary<string, string> locations, IDecoder decoder);
}