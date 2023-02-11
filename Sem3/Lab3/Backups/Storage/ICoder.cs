using Backups.Utilities;

namespace Backups.Storages;

public interface IEncoder
{
    void Encode(IRepoObject obj, Stream outputStream);
}