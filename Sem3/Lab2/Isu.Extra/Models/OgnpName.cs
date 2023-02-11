using Isu.Exceptions;
using Isu.Models;

namespace Isu.Extra.Models;

public class OgnpName
{
    public OgnpName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new FormatException("Can't infer OGNP group name from an empty string.");

        if (value[0] < 'A' || value[0] > 'Z')
            throw new FormatException("OGNP group name must start with a prefix");

        Value = value;
    }

    public string Value { get; }

    public char FacultyCode => Value[0];

    public override bool Equals(object? obj)
    {
        if ((obj is null) || !(obj is OgnpName))
            return false;
        return ((OgnpName)obj).Value.Equals(Value);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;
}