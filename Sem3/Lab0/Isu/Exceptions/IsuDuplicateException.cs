namespace Isu.Exceptions;

public class IsuDuplicateException : Exception
{
    public IsuDuplicateException()
        { }

    public IsuDuplicateException(string message)
        : base(message) { }

    public IsuDuplicateException(string message, Exception inner)
        : base(message, inner) { }
}