using Isu.Exceptions;
using Isu.Models;

namespace Isu.Entities;

public class Group
{
    public const int MaxStudents = 25;
    private List<Student> _students = new List<Student>();

    public Group(GroupName name, Course course)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(course);

        if (course.Number.Year != name.Year)
            throw new IsuException("Can't create Group: Course years between GroupName and Course don't match.");

        (Name, Course) = (name, course);
    }

    public GroupName Name { get; }
    public Course Course { get; }

    public IReadOnlyList<Student> Students => _students;

    public bool HasStudent(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        return _students.Contains(student);
    }

    public void AddStudent(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        if (_students.Contains(student))
            return;

        if (_students.Count == MaxStudents)
            throw new IsuOvercrowdException($"Too much students (max {MaxStudents}).");

        _students.Add(student);
        student.ChangeGroup(this);
    }

    public void RemoveStudent(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        _students.Remove(student);
    }

    public override bool Equals(object? obj)
    {
        if ((obj is null) || !(obj is Group))
            return false;
        return Name.Equals(((Group)obj).Name);
    }

    public override int GetHashCode() => Name.GetHashCode();
}