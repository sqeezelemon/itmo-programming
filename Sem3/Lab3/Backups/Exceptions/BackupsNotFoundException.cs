using System;
namespace Backups.Exceptions;

public class BackupsNotFoundException : Exception
{
    public BackupsNotFoundException()
    {
    }

    public BackupsNotFoundException(string message)
        : base(message)
    {
    }

    public BackupsNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}