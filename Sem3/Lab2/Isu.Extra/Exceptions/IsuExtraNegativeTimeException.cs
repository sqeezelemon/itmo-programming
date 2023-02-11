namespace Isu.Extra.Exceptions;

public class IsuExtraNegativeTimeException : Exception
{
    public IsuExtraNegativeTimeException()
        { }

    public IsuExtraNegativeTimeException(string message)
        : base(message) { }

    public IsuExtraNegativeTimeException(string message, Exception inner)
        : base(message, inner) { }
}