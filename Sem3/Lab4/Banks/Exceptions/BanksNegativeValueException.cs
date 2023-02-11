using System;
namespace Banks.Exceptions;

public class BanksNegativeValueException : Exception
{
    public BanksNegativeValueException()
    {
    }

    public BanksNegativeValueException(string message)
        : base(message)
    {
    }

    public BanksNegativeValueException(string message, System.Exception inner)
        : base(message, inner)
    {
    }

    public static void ThrowIfNegative<T>(T value)
        where T : IComparable
    {
        if (value.CompareTo(default(T)) < 0)
            throw new BanksNegativeValueException();
    }
}