using Isu.Exceptions;
using Isu.Models;

namespace Isu.Entities;

public class Course
{
    private List<Group> _groups = new List<Group>();

    public Course(CourseNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);
        Number = number;
    }

    public IReadOnlyList<Group> Groups => _groups;
    public CourseNumber Number { get; }

    public bool HasGroup(Group group) => _groups.Contains(group);

    public void AddGroup(Group group)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (group.Name.Year != Number.Year)
            throw new IsuException($"Year {group.Name.Year} group can't be added to year {Number.Year} Course.");

        if (this.HasGroup(group))
            return;

        _groups.Add(group);
    }

    public override bool Equals(object? obj)
    {
        if ((obj is null) || !(obj is Course))
            return false;
        return Number.Equals(((Course)obj).Number);
    }

    public override int GetHashCode() => Number.Year;
}