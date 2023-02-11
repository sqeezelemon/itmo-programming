using System;
namespace Backups.Exceptions;

public class BackupsDuplicateException : Exception
{
    public BackupsDuplicateException()
    {
    }

    public BackupsDuplicateException(string message)
        : base(message)
    {
    }

    public BackupsDuplicateException(string message, Exception inner)
        : base(message, inner)
    {
    }
}