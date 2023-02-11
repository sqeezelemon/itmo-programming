namespace Isu.Extra.Exceptions;

public class IsuExtraDuplicateFaculty : Exception
{
    public IsuExtraDuplicateFaculty()
        { }

    public IsuExtraDuplicateFaculty(string message)
        : base(message) { }

    public IsuExtraDuplicateFaculty(string message, Exception inner)
        : base(message, inner) { }
}