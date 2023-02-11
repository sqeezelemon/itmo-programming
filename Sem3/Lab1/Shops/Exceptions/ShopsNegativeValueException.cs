namespace Shops.Exceptions;

public class ShopsNegativeValueException : Exception
{
    public ShopsNegativeValueException()
        { }

    public ShopsNegativeValueException(string message)
        : base(message) { }

    public ShopsNegativeValueException(string message, Exception inner)
        : base(message, inner) { }
}