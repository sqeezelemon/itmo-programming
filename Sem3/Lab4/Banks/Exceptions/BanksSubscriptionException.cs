using System;
namespace Banks.Exceptions;

public class BanksSubscriptionException : Exception
{
    public BanksSubscriptionException()
    {
    }

    public BanksSubscriptionException(string message)
        : base(message)
    {
    }

    public BanksSubscriptionException(string message, System.Exception inner)
        : base(message, inner)
    {
    }
}