namespace Reports.App.Exceptions;

public class ReportsNotFoundException : Exception
{
    public ReportsNotFoundException()
    {
    }

    public ReportsNotFoundException(string message)
        : base(message)
    {
    }

    public ReportsNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}