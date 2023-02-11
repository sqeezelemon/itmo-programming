namespace Isu.Exceptions;

public class IsuNotFoundException : Exception
{
    public IsuNotFoundException()
        { }

    public IsuNotFoundException(string message)
        : base(message) { }

    public IsuNotFoundException(string message, Exception inner)
        : base(message, inner) { }
}