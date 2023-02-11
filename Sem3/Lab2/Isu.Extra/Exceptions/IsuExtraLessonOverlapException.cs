namespace Isu.Extra.Exceptions;

public class IsuExtraLessonOverlapException : Exception
{
    public IsuExtraLessonOverlapException()
        { }

    public IsuExtraLessonOverlapException(string message)
        : base(message) { }

    public IsuExtraLessonOverlapException(string message, Exception inner)
        : base(message, inner) { }
}