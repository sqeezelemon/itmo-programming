using Isu.Entities;
using Isu.Exceptions;
using Isu.Extra.Entities;
using Isu.Extra.Exceptions;
using Isu.Extra.Models;
using Isu.Models;
using Isu.Services;

namespace Isu.Extra.Service;

public class IsuExtraService
{
    private IsuService isu = new IsuService();
    private List<ExtraGroup> _groups = new List<ExtraGroup>();
    private List<ExtraStudent> _students = new List<ExtraStudent>();
    private List<Ognp> _ognps = new List<Ognp>();

    public IsuExtraService() { }

    public Ognp AddOgnp(OgnpName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (HasOgnp(name))
            throw new IsuException($"OGNP with name {name} already exists.");

        Ognp ognp = new Ognp(name);
        _ognps.Add(ognp);
        return ognp;
    }

    public void AddStudentToOgnp(OgnpGroup ognpGroup, ExtraStudent student)
    {
        ArgumentNullException.ThrowIfNull(ognpGroup);
        ArgumentNullException.ThrowIfNull(student);

        if (!HasOgnpGroup(ognpGroup))
            throw new IsuNotFoundException($"OGNP group {ognpGroup} not found");

        if (!HasStudent(student))
            throw new IsuNotFoundException($"Student {student} not found");

        ExtraGroup extraGroup = FindGroup(student.Group.Name) !;
        if (extraGroup.Schedule.Overlaps(ognpGroup.Schedule)
            || student.OgnpGroups.Any(g => g.Schedule.Overlaps(ognpGroup.Schedule)))
            throw new IsuExtraLessonOverlapException("OGNP schedule overlaps with existing schedule");

        student.AddOgnp(ognpGroup);
    }

    public void RemoveStudentFromOgnp(OgnpGroup group, ExtraStudent student)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(student);

        if (!_ognps.Any(o => o.HasGroup(group)))
            throw new IsuNotFoundException($"OGNP group {group} not found");

        if (!HasStudent(student))
            throw new IsuNotFoundException($"Student {student} not found");

        group.RemoveStudent(student);
        student.RemoveOgnp(group);
    }

    public IReadOnlyList<OgnpGroup> FindGroupsForOgnp(Ognp ognp)
    {
        ArgumentNullException.ThrowIfNull(ognp);
        if (!_ognps.Contains(ognp))
            throw new IsuNotFoundException($"OGNP {ognp} not found.");
        return ognp.Groups;
    }

    public IReadOnlyList<ExtraStudent> GetStudentsForOgnpGroup(OgnpGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (!HasOgnpGroup(group))
            throw new IsuNotFoundException($"OGNP group {group} not found.");
        return group.Students;
    }

    public IReadOnlyList<ExtraStudent> GetStudentsWithoutOgnp(ExtraGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (!HasGroup(group))
            throw new IsuNotFoundException($"Group {group} not found.");
        var res =
            group.ExtraStudents
            .Where(s => s.OgnpGroups.Count == 0)
            .ToList();
        return res;
    }

    public OgnpGroup AddOgnpGroup(OgnpName name, Ognp ognp)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(ognp);

        if (_ognps.Any(o => o.Groups.Any(g => g.Name == name)))
            throw new IsuDuplicateException($"Group with name {name} already exists.");

        if (!_ognps.Contains(ognp))
            throw new IsuNotFoundException("OGNP not found in current context.");

        return ognp.AddGroup(name);
    }

    public ExtraGroup AddGroup(GroupName name)
    {
        Group group = isu.AddGroup(name);
        ExtraGroup extraGroup = new ExtraGroup(group.Name, group.Course);
        _groups.Add(extraGroup);
        return extraGroup;
    }

    public ExtraStudent AddStudent(ExtraGroup group, string name)
    {
        ArgumentNullException.ThrowIfNull(group);
        if (!HasGroup(group))
            throw new IsuNotFoundException($"ExtraGroup with Group {group} not found");
        Group oldGroup = isu.FindGroup(group.Name) !;
        Student student = isu.AddStudent(oldGroup, name);
        ExtraStudent extraStudent = new ExtraStudent(student.Id, student.Name, group);
        _students.Add(extraStudent);
        return extraStudent;
    }

    public ExtraStudent GetStudent(int id)
    {
        ExtraStudent? student = FindStudent(id);
        if (student is null)
            throw new IsuNotFoundException($"Student with ID {id} not found.");
        return student!;
    }

    public ExtraStudent? FindStudent(int id) => _students.Find(s => s.Id == id);

    public IReadOnlyList<ExtraStudent> FindStudents(GroupName groupName)
    {
        ExtraGroup? group = _groups.Find(g => g.Name == groupName);
        if (group is null)
            throw new IsuNotFoundException("Group not found.");
        return group.ExtraStudents;
    }

    public IReadOnlyList<ExtraStudent> FindStudents(CourseNumber courseNumber) => _students.Where(s => s.Group.Name.Year == courseNumber.Year).ToList();

    public ExtraGroup? FindGroup(GroupName groupName) => _groups.Find(g => g.Name == groupName);

    public IReadOnlyList<ExtraGroup> FindGroups(CourseNumber courseNumber) => _groups.Where(g => g.Name.Year == courseNumber.Year).ToList();

    public void ChangeStudentGroup(ExtraStudent student, ExtraGroup newGroup)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(newGroup);

        if (!HasStudent(student))
            throw new IsuNotFoundException("Student not found");

        if (!HasGroup(newGroup))
            throw new IsuNotFoundException("New group not found");

        ExtraGroup oldGroup = student.ExtraGroup;
        isu.ChangeStudentGroup(student, newGroup);
        oldGroup.RemoveStudent(student);
        newGroup.AddStudent(student);
    }

    internal bool HasStudent(ExtraStudent student) => _students.Contains(student);
    internal bool HasGroup(ExtraGroup group) => _groups.Contains(group);
    internal bool HasOgnp(OgnpName name) => _ognps.Any(o => o.Name == name);
    internal bool HasOgnpGroup(OgnpGroup group) => _ognps.Any(o => o.HasGroup(group));
}