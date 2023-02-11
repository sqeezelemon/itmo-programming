using Backups.Exceptions;
namespace Backups.Utilities;

public class MemoryFolder : IFolder
{
    private List<IRepoObject> _contents = new List<IRepoObject>();
    public MemoryFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new FormatException("Can't infer path from empty string");
        Path = path;
    }

    public IReadOnlyList<IRepoObject> Contents => _contents;
    public string Path { get; }

    internal void AddObject(IRepoObject newObj)
    {
        if (_contents.Any(obj => obj.Path == newObj.Path))
            throw new BackupsDuplicateException($"Object with name {newObj.Path} already exists");
        if (!newObj.Path.StartsWith(Path))
            throw new BackupsInvalidPathException($"Can't add object with path {newObj.Path} in {Path}");
        _contents.Add(newObj);
    }

    internal void RemoveObject(string path)
    {
        if (_contents.RemoveAll(obj => obj.Path == path) < 0)
            throw new BackupsNotFoundException($"Can't remove object with path {path} because it is nonexistent.");
    }
}