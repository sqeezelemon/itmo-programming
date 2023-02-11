namespace Reports.App.Exceptions;

public class ReportsDuplicateException : Exception
{
    public ReportsDuplicateException()
    {
    }

    public ReportsDuplicateException(string message)
        : base(message)
    {
    }

    public ReportsDuplicateException(string message, Exception inner)
        : base(message, inner)
    {
    }
}