namespace Isu.Exceptions;

public class IsuOvercrowdException : Exception
{
    public IsuOvercrowdException()
        { }

    public IsuOvercrowdException(string message)
        : base(message) { }

    public IsuOvercrowdException(string message, Exception inner)
        : base(message, inner) { }
}