using Backups.Exceptions;
namespace Backups.Utilities;

public class MemoryRepository : IRepository
{
    private MemoryFolder _root = new MemoryFolder("mem");

    public MemoryRepository()
    {
    }

    public IFile MakeFile(string path)
    {
        var parent = GetParent(path);
        var file = new MemoryFile(path);
        parent.AddObject(file);
        return file;
    }

    public IFolder MakeFolder(string path)
    {
        var parent = GetParent(path);
        var folder = new MemoryFolder(path);
        parent.AddObject(folder);
        return folder;
    }

    public IRepoObject GetObject(string path) => FindObject(path) ?? throw new BackupsNotFoundException($"Object at path {path} not found");

    public IRepoObject? FindObject(string path)
    {
        var pathComps = path.Split(Path.DirectorySeparatorChar);
        if (pathComps is null || pathComps.Length == 0)
            throw new BackupsInvalidPathException($"Can't infer path components from {path}");

        MemoryFolder currObj = _root;
        string currPath = currObj.Path;
        int max = pathComps.Length - 1; // - ((pathComps.Last() == string.Empty) ? 1 : 0);

        for (int i = 1; i <= max; i++)
        {
            currPath = Path.Combine(currPath, pathComps[i]);
            var obj = currObj.Contents.SingleOrDefault(ro => ro.Path == currPath);
            if (!(obj is MemoryFolder))
            {
                if (obj is null)
                    return null;
                else if (obj.Path == path)
                    return obj;
                return null;
            }

            currObj = (MemoryFolder)obj;
        }

        return currObj;
    }

    public void Delete(string path)
    {
        var parent = GetParent(path);
        parent.RemoveObject(path);
    }

    internal MemoryFolder GetParent(string path)
    {
        var parentPath = Path.GetDirectoryName(path);
        if (parentPath is null)
            throw new BackupsInvalidPathException($"Can't infer parent path from {path}");

        var parentDir = GetObject(parentPath!) as MemoryFolder;
        if (parentDir is null)
            throw new BackupsInvalidPathException($"Can't find directory for {path}");

        return parentDir;
    }
}