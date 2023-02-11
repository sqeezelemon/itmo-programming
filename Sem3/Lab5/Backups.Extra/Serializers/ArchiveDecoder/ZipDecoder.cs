using System.IO.Compression;
using Backups.Utilities;

namespace Backups.Extra.Serializers;

public class ZipDecoder : IDecoder
{
    public void Decode(IFile source, IFolder destination, IRepository repository)
    {
        using var archiveStream = new MemoryStream();
        source.Read(archiveStream);
        archiveStream.Seek(0, SeekOrigin.Begin);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);
        foreach (var entry in archive.Entries)
        {
            var path = entry.Name;
            var pathComps = path.Split(Path.DirectorySeparatorChar);
            var currPath = destination.Path;
            for (int i = 0; i < pathComps.Length - 1; i++)
            {
                var component = pathComps[i];
                currPath += Path.DirectorySeparatorChar + component;
                try
                {
                    var obj = repository.GetObject(currPath);
                    if (obj is IFile)
                        break;
                }
                catch
                {
                    repository.MakeFolder(currPath);
                }
            }

            var file = repository.MakeFile(destination.Path + Path.DirectorySeparatorChar + path);
            var entryStream = entry.Open();
            file.Write(entryStream);
            entryStream.Close();
        }
    }
}