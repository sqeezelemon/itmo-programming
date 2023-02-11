using Isu.Exceptions;

namespace Isu.Entities;

public class Student
{
    public Student(int id, string name, Group group)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(group);

        (Id, Name, Group) = (id, name, group);
    }

    public int Id { get; }
    public string Name { get; }
    public Group Group { get; private set; }

    public void ChangeGroup(Group newGroup)
    {
        ArgumentNullException.ThrowIfNull(newGroup);

        if (!newGroup.HasStudent(this))
            throw new IsuCantTransferException("Group does not contain student.");

        Group = newGroup;
    }

    public override bool Equals(object? obj)
    {
        if ((obj is null) || !(obj is Student))
            return false;
        return Id.Equals(((Student)obj).Id);
    }

    public override int GetHashCode() => Id;
}