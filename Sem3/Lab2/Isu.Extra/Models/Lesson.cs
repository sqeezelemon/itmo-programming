using Isu.Extra.Exceptions;

namespace Isu.Extra.Models;

public class Lesson
{
    // Min length in minutes
    public const int MinLength = 30;

    // Max length in minutes
    public const int MaxLength = 180;

    public Lesson(string name, TimeOnly start, TimeOnly end)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Can't infer lesson name from an empty string.");

        if (start > end)
            throw new IsuExtraNegativeTimeException("Start time can't be after end");
        if ((end - start).Minutes < MinLength || (end - start).Minutes > MaxLength)
            throw new IsuExtraNegativeTimeException($"Length {(end - start).Minutes} ({MinLength} min. - {MaxLength} min. allowed)");

        (Name, Start, End) = (name, start, end);
    }

    public string Name { get; }
    public TimeOnly Start { get; private set; }
    public TimeOnly End { get; private set; }

    public bool Overlaps(Lesson other)
    {
        if (other is null)
            return false;
        return ((Start <= other.Start) && (other.Start <= End))
            || ((Start <= other.End) && (other.End <= End));
    }

    public void Reschedule(TimeOnly start, TimeOnly end)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);

        if (start > end)
            throw new IsuExtraNegativeTimeException($"Start time {start} can't be after end time {end}.");

        (Start, End) = (start, end);
    }
}