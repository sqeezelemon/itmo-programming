namespace Reports.App.Exceptions;

public class ReportsCredentialsException : Exception
{
    public ReportsCredentialsException()
    {
    }

    public ReportsCredentialsException(string message)
        : base(message)
    {
    }

    public ReportsCredentialsException(string message, Exception inner)
        : base(message, inner)
    {
    }
}