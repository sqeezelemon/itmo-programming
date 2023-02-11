namespace Shops.Exceptions;

public class ShopsNotEnoughInventoryException : Exception
{
    public ShopsNotEnoughInventoryException()
        { }

    public ShopsNotEnoughInventoryException(string message)
        : base(message) { }

    public ShopsNotEnoughInventoryException(string message, Exception inner)
        : base(message, inner) { }
}