using Backups.Exceptions;
namespace Backups.Utilities;

public class DiskRepository : IRepository
{
    public DiskRepository()
    {
    }

    public IFile MakeFile(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new FormatException("Can't infer filepath from empty string");

        if (!Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute))
            throw new FormatException("Invalid path");

        var di = new DirectoryInfo(Path.GetDirectoryName(path) !);
        if (!di.Exists)
            di.Create();

        var fi = new FileInfo(path);
        if (!fi.Exists)
            fi.Create().Dispose();

        return new DiskFile(path);
    }

    public IFolder MakeFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new FormatException("Can't infer filepath from empty string");

        if (!Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute))
            throw new FormatException("Invalid path");

        var di = new DirectoryInfo(Path.GetDirectoryName(path) !);
        if (!di.Exists)
            di.Create();

        return new DiskFolder(path);
    }

    public IRepoObject GetObject(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new FormatException("Can't infer filepath from empty string");

        if (Directory.Exists(path))
            return new DiskFolder(path);
        else if (File.Exists(path))
            return new DiskFile(path);
        else
            throw new BackupsNotFoundException("Not found");
    }

    public void Delete(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        else if (File.Exists(path))
            File.Delete(path);
        else
            throw new BackupsNotFoundException("Not found");
    }
}