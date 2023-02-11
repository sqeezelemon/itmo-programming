using System;
namespace Backups.Utilities;

public class DiskFolder : IFolder
{
    public DiskFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new FormatException("Can't infer filepath from empty string");
        Path = path;
    }

    public string Path { get; }

    public IReadOnlyList<IRepoObject> Contents
    {
        get
        {
            var files = Directory.EnumerateFiles(Path)
                .Select(p => new DiskFile(p))
                .ToList();
            var folders = Directory.EnumerateDirectories(Path)
                .Select(p => new DiskFolder(p))
                .ToList();
            var res = new List<IRepoObject>();
            res.AddRange(files);
            res.AddRange(folders);
            return res;
        }
    }
}