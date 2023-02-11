using Isu.Exceptions;
using Isu.Extra.Models;

namespace Isu.Extra.Entities;

public class Ognp
{
    private List<OgnpGroup> _groups = new List<OgnpGroup>();

    public Ognp(OgnpName name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
    }

    public OgnpName Name { get; }
    public IReadOnlyList<OgnpGroup> Groups => _groups;

    public OgnpGroup AddGroup(OgnpName name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (HasGroup(name))
            throw new IsuDuplicateException($"OGNP group with name {name} already exists.");

        OgnpGroup group = new OgnpGroup(name, this);
        _groups.Add(group);
        return group;
    }

    public bool HasGroup(OgnpName name) => _groups.Any(g => g.Name == name);

    public bool HasGroup(OgnpGroup group) => _groups.Contains(group);
}