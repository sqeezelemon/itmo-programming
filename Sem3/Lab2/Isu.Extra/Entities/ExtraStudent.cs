using Isu.Entities;
using Isu.Exceptions;
using Isu.Extra.Exceptions;

namespace Isu.Extra.Entities;

public class ExtraStudent : Student
{
    public const int MaxOgnp = 2;

    private List<OgnpGroup> _ognpGroups = new List<OgnpGroup>();

    public ExtraStudent(int id, string name, ExtraGroup group)
        : base(id, name, group)
    {
        ExtraGroup = group;
    }

    public IReadOnlyList<OgnpGroup> OgnpGroups => _ognpGroups;
    public ExtraGroup ExtraGroup { get; private set; }

    public void AddOgnp(OgnpGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (HasOgnp(group.Ognp))
            throw new IsuDuplicateException($"Student is already in OGNP group {group}.");

        if (HasCourseFromFaculty(group.Name.FacultyCode))
            throw new IsuExtraDuplicateFaculty($"Student already has OGNP from faculty {group.Name.FacultyCode}");

        if (Group.Name.ToString()[0] == group.Name.FacultyCode)
            throw new IsuExtraDuplicateFaculty($"Student from faculty {group.Name.FacultyCode} can't go to OGNP from the same faculty.");

        if (OgnpGroups.Count == MaxOgnp)
            throw new IsuOgnpOverloadException($"Student reached OGNP limit {MaxOgnp}.");

        _ognpGroups.Add(group);
    }

    public void RemoveOgnp(OgnpGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (!_ognpGroups.Remove(group))
            throw new IsuNotFoundException($"OGNP {group} not found.");
    }

    public void ChangeExtraGroup(ExtraGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        ExtraGroup = group;
    }

    public bool HasCourseFromFaculty(char facultyCode) => OgnpGroups.Any(g => g.Name.FacultyCode == facultyCode);
    public bool HasOgnp(Ognp ognp) => OgnpGroups.Any(g => g.Ognp == ognp);
}