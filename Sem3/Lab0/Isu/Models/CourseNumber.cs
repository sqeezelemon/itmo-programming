using Isu.Exceptions;

namespace Isu.Models;

public class CourseNumber
{
    public const int Min = 1;
    public const int Max = 7;

    public CourseNumber(int year)
    {
        ArgumentNullException.ThrowIfNull(year);

        if (year < Min || year > Max)
            throw new IsuException($"{year} is an invalid year (1 to {Max} allowed).");

        Year = year;
    }

    public CourseNumber(GroupName groupName)
    {
        ArgumentNullException.ThrowIfNull(groupName);

        int year = groupName.Year;
        if (year < Min || year > Max)
            throw new IsuException($"{year} is an invalid year (1 to {Max} allowed).");

        Year = year;
    }

    public int Year { get; }

    public override bool Equals(object? obj)
    {
        if ((obj is null) || !(obj is CourseNumber))
            return false;
        return Year.Equals(((CourseNumber)obj).Year);
    }

    public override int GetHashCode() => Year;
}