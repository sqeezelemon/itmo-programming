namespace Banks.Exceptions;

public class BanksInvalidValueException : Exception
{
    public BanksInvalidValueException()
    {
    }

    public BanksInvalidValueException(string message)
        : base(message)
    {
    }

    public BanksInvalidValueException(string message, System.Exception inner)
        : base(message, inner)
    {
    }
}