namespace Shops.Exceptions;

public class ShopsTooPoorException : Exception
{
    public ShopsTooPoorException()
        { }

    public ShopsTooPoorException(string message)
        : base(message) { }

    public ShopsTooPoorException(string message, Exception inner)
        : base(message, inner) { }
}