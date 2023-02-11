namespace Shops.Exceptions;

public class ShopsNotFoundException : Exception
{
    public ShopsNotFoundException()
        { }

    public ShopsNotFoundException(string message)
        : base(message) { }

    public ShopsNotFoundException(string message, Exception inner)
        : base(message, inner) { }
}