using System;
namespace Backups.Exceptions;

public class BackupsInvalidPathException : Exception
{
    public BackupsInvalidPathException()
    {
    }

    public BackupsInvalidPathException(string message)
        : base(message)
    {
    }

    public BackupsInvalidPathException(string message, Exception inner)
        : base(message, inner)
    {
    }
}