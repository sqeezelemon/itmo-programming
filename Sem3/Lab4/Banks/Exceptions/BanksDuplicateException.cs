using System;
namespace Banks.Exceptions;

public class BanksDuplicateException : Exception
{
    public BanksDuplicateException()
    {
    }

    public BanksDuplicateException(string message)
        : base(message)
    {
    }

    public BanksDuplicateException(string message, System.Exception inner)
        : base(message, inner)
    {
    }
}