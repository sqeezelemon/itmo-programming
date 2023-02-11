namespace Isu.Extra.Exceptions;

public class IsuOgnpOverloadException : Exception
{
    public IsuOgnpOverloadException()
        { }

    public IsuOgnpOverloadException(string message)
        : base(message) { }

    public IsuOgnpOverloadException(string message, Exception inner)
        : base(message, inner) { }
}