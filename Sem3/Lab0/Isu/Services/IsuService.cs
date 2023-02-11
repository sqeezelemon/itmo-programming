using System.Xml.Linq;
using Isu.Entities;
using Isu.Exceptions;
using Isu.Models;

namespace Isu.Services;

public class IsuService : IIsuService
{
    private int _nextStudentId = 100000;

    private List<Course> _courses = new List<Course>();
    private List<Group> _groups = new List<Group>();
    private List<Student> _students = new List<Student>();

    public Group AddGroup(GroupName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        Group? group = FindGroup(name);
        if (group != null)
            throw new IsuDuplicateException($"Group with name {name} already exists.");

        CourseNumber number = new CourseNumber(name);
        Course course = GetOrCreateCourse(number);
        group = new Group(name, course);
        course.AddGroup(group);
        _groups.Add(group);
        return group;
    }

    public Student AddStudent(Group group, string name)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (!HasGroup(group))
            throw new IsuNotFoundException($"Group {group.Name} not found.");

        Student student = new Student(_nextStudentId, name, group);
        group.AddStudent(student);
        _students.Add(student);
        ++_nextStudentId;
        return student;
    }

    public Student GetStudent(int id)
    {
        Student? student = FindStudent(id);
        if (student == null)
            throw new IsuNotFoundException($"Student {id} not found.");
        return student!;
    }

    public Student? FindStudent(int id) => _students.SingleOrDefault(s => s.Id == id);

    public IReadOnlyList<Student> FindStudents(GroupName groupName)
    {
        ArgumentNullException.ThrowIfNull(groupName);

        Group? group = FindGroup(groupName);
        if (group == null)
            throw new IsuNotFoundException($"Group {groupName} not found.");
        return group!.Students;
    }

    public IReadOnlyList<Student> FindStudents(CourseNumber courseNumber)
    {
        ArgumentNullException.ThrowIfNull(courseNumber);

        var query =
            _courses
            .Where(c => c.Number == courseNumber)
            .SelectMany(c => c.Groups)
            .SelectMany(g => g.Students);

        List<Student> result = query.ToList();
        return result;
    }

    public Group? FindGroup(GroupName groupName) => _groups.SingleOrDefault(g => g.Name == groupName);

    public IReadOnlyList<Group> FindGroups(CourseNumber courseNumber)
    {
        ArgumentNullException.ThrowIfNull(courseNumber);

        Course? course = _courses.SingleOrDefault(c => c.Number == courseNumber);
        if (course == null)
            return new List<Group>();
        return course!.Groups;
    }

    public void ChangeStudentGroup(Student student, Group newGroup)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(newGroup);

        if (student.Group == newGroup)
            throw new IsuCantTransferException($"Student {student.Id} is already in group {newGroup.Name}.");

        if (!HasStudent(student))
            throw new IsuNotFoundException($"Student {student.Id} not found.");

        if (!HasGroup(newGroup))
            throw new IsuNotFoundException($"Group {newGroup.Name} not found.");

        Group oldGroup = student.Group;
        newGroup.AddStudent(student);
        oldGroup.RemoveStudent(student);
    }

    private Course GetOrCreateCourse(CourseNumber number)
    {
        Course? course = _courses.SingleOrDefault(c => c.Number == number);
        if (course == null)
        {
            course = new Course(number);
            _courses.Add(course!);
        }

        return course!;
    }

    private bool HasGroup(Group group) => _groups.Any(g => g == group);

    private bool HasStudent(Student student) => _students.Contains(student);
}