using System;
namespace Banks.Exceptions;

public class BanksNotFoundException : Exception
{
    public BanksNotFoundException()
    {
    }

    public BanksNotFoundException(string message)
        : base(message)
    {
    }

    public BanksNotFoundException(string message, System.Exception inner)
        : base(message, inner)
    {
    }
}