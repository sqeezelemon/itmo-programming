using System;
namespace Backups.Utilities;

public class MemoryFile : IFile, IDisposable
{
    private MemoryStream _fstream = new MemoryStream();

    public MemoryFile(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new FormatException("Can't infer filepath from empty string");
        Path = path;
    }

    public string Path { get; }

    public void Read(Stream outputStream)
    {
        ArgumentNullException.ThrowIfNull(outputStream);
        _fstream.Seek(0, SeekOrigin.Begin);
        _fstream.CopyTo(outputStream);
    }

    public void Write(Stream inputStream)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        _fstream.SetLength(0);
        inputStream.CopyTo(_fstream);
    }

    public void Append(Stream inputStream)
    {
        inputStream.CopyTo(_fstream);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            _fstream.Close();
    }
}