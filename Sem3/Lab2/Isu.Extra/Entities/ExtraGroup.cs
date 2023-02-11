using Isu.Entities;
using Isu.Exceptions;
using Isu.Models;

namespace Isu.Extra.Entities;

public class ExtraGroup : Group
{
    private List<ExtraStudent> _extraStudents = new List<ExtraStudent>();

    public ExtraGroup(GroupName name, Course course)
        : base(name, course)
    {
        Schedule = new Schedule();
    }

    public Schedule Schedule { get; }
    public IReadOnlyList<ExtraStudent> ExtraStudents => _extraStudents;

    public void AddExtraStudent(ExtraStudent student)
    {
        ArgumentNullException.ThrowIfNull(student);
        _extraStudents.Add(student);
    }

    public void RemoveExtraStudent(ExtraStudent student)
    {
        ArgumentNullException.ThrowIfNull(student);

        if (!_extraStudents.Remove(student))
            throw new IsuNotFoundException($"Student {student} not found.");
    }

    public bool HasStudent(ExtraStudent student) => _extraStudents.Contains(student);
}