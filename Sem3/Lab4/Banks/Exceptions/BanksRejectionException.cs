using System;
namespace Banks.Exceptions;

public class BanksRejectionException : Exception
{
    public BanksRejectionException()
    {
    }

    public BanksRejectionException(string message)
        : base(message)
    {
    }

    public BanksRejectionException(string message, System.Exception inner)
        : base(message, inner)
    {
    }
}