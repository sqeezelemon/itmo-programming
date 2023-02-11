using Backups.Utilities;

namespace Backups.Extra.Serializers;

public interface IDecoder
{
    void Decode(IFile source, IFolder destination, IRepository repository);
}