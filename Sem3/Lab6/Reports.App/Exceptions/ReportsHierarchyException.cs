namespace Reports.App.Exceptions;

public class ReportsHierarchyException : Exception
{
    public ReportsHierarchyException()
    {
    }

    public ReportsHierarchyException(string message)
        : base(message)
    {
    }

    public ReportsHierarchyException(string message, Exception inner)
        : base(message, inner)
    {
    }
}