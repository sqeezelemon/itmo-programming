using System.IO.Compression;
using Backups.Exceptions;
using Backups.Utilities;

namespace Backups.Storages;

public class ZipEncoder : IEncoder
{
    public void Encode(IRepoObject obj, Stream outputStream)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(outputStream);

        var directory = Path.GetDirectoryName(obj.Path);
        if (directory is null)
            throw new BackupsInvalidPathException($"Can't infer folder name from {obj.Path}");

        using var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true);

        if (obj is IFolder)
        {
            int offset = obj.Path.Length + ((obj.Path.Last() == Path.DirectorySeparatorChar) ? 1 : 0);
            EncodeDir(offset, (obj as IFolder) !, archive);
        }
        else if (obj is IFile)
        {
            int offset = directory!.Length + 1;
            EncodeFile(offset, (obj as IFile) !, archive);
        }
    }

    private void EncodeDir(int offset, IFolder folder, ZipArchive archive)
    {
        foreach (IRepoObject obj in folder.Contents)
        {
            if (obj is IFolder)
            {
                EncodeDir(offset, (obj as IFolder) !, archive);
                continue;
            }
            else if (obj is IFile)
            {
                EncodeFile(offset, (obj as IFile) !, archive);
            }
        }
    }

    private void EncodeFile(int offset, IFile file, ZipArchive archive)
    {
        var entry = archive.CreateEntry(file.Path.Substring(offset));
        using var entryStream = entry.Open();
        file.Read(entryStream);
    }
}