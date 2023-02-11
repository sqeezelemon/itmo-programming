using System.Text;
using Backups.Utilities;

namespace Backups.Extra.Utils;

public class FileLogger : Logger
{
    public FileLogger(IFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        LogFile = file;
    }

    private IFile LogFile { get; }

    protected override void Write(string message)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(message + "\n"));
        LogFile.Append(stream);
    }
}