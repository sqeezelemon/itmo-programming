using System;
namespace Backups.Utilities;

public class DiskFile : IFile
{
    public DiskFile(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new FormatException("Can't infer filepath from empty string");
        Path = path;
    }

    public string Path { get; }

    public void Read(Stream outputStream)
    {
        FileStream fstream = new FileStream(Path, FileMode.Open);

        fstream.CopyTo(outputStream);
        fstream.Close();
    }

    public void Write(Stream inputStream)
    {
        FileStream fstream = new FileStream(Path, FileMode.Create);

        // inputStream.Seek(0, SeekOrigin.Begin);
        inputStream.CopyTo(fstream);
        fstream.Flush();
        fstream.Close();
    }

    public void Append(Stream inputStream)
    {
        FileStream fstream = new FileStream(Path, FileMode.Append);
        inputStream.CopyTo(fstream);
        fstream.Flush();
        fstream.Close();
    }
}