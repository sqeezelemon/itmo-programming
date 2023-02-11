using Backups.Exceptions;
using Backups.Utilities;

namespace Backups.Backups;

public class BackupObject : IBackupObject
{
    public BackupObject(string name, IRepoObject value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) > 0)
            throw new BackupsInvalidPathException($"Name {name} contains illegal characters.");

        (Name, Value) = (name, value);
    }

    public string Name { get; }
    public IRepoObject Value { get; }

    public IBackupObject GetCopy() => (MemberwiseClone() as BackupObject) !;
}