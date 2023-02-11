namespace Isu.Exceptions;

public class IsuCantTransferException : Exception
{
    public IsuCantTransferException()
    { }

    public IsuCantTransferException(string message)
        : base(message) { }

    public IsuCantTransferException(string message, Exception inner)
        : base(message, inner) { }
}