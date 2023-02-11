using Backups.Utilities;
namespace Backups.Storages;

public class Storage
{
    public Storage(IRepository repository, IFolder location)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(location);
        (Repository, Location) = (repository, location);
    }

    public IRepository Repository { get; }
    public IFolder Location { get; }
}