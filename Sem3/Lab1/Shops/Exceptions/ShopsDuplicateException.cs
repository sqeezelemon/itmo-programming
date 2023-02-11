namespace Shops.Exceptions;

public class ShopsDuplicateException : Exception
{
    public ShopsDuplicateException()
        { }

    public ShopsDuplicateException(string message)
        : base(message) { }

    public ShopsDuplicateException(string message, Exception inner)
        : base(message, inner) { }
}