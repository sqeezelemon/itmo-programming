using Isu.Entities;
using Isu.Exceptions;
using Isu.Extra.Models;

namespace Isu.Extra.Entities;

public class OgnpGroup
{
    public const int MaxStudents = 25;

    private List<ExtraStudent> _students = new List<ExtraStudent>();

    public OgnpGroup(OgnpName name, Ognp ognp)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(ognp);
        (Ognp, Name) = (ognp, name);
        Schedule = new Schedule();
    }

    public Schedule Schedule { get; private set; }
    public OgnpName Name { get; }
    public Ognp Ognp { get; }
    public IReadOnlyList<ExtraStudent> Students => _students;

    public void AddStudent(ExtraStudent student)
    {
        ArgumentNullException.ThrowIfNull(student);

        if (_students.Contains(student))
            throw new IsuDuplicateException($"Student {student} already in group.");

        if (_students.Count == MaxStudents)
            throw new IsuOvercrowdException($"Too much students (max {MaxStudents}).");

        _students.Add(student);
    }

    public void RemoveStudent(ExtraStudent student)
    {
        ArgumentNullException.ThrowIfNull(student);
        if (!_students.Remove(student))
            throw new IsuNotFoundException($"Can't remove student {student}: not found.");
    }
}