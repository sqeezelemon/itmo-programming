namespace Isu.Exceptions;

public class IsuException : Exception
{
    public IsuException()
        { }

    public IsuException(string message)
        : base(message) { }

    public IsuException(string message, Exception inner)
        : base(message, inner) { }
}