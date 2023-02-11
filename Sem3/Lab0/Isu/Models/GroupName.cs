namespace Isu.Models;

public class GroupName
{
    public const int MinNumber = 1000;

    private char _prefix;

    public GroupName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new FormatException("Can't infer GroupName from an empty string.");

        if (name[0] >= '0' && name[0] <= '9')
            throw new FormatException($"Can't infer group prefix from {name}");
        _prefix = name[0];

        string numstr = name.Substring(1);
        if (!int.TryParse(numstr, out int number))
            throw new FormatException($"Can't parse group number from {numstr}.");

        if (number < MinNumber)
            throw new FormatException($"Group number {numstr} is too small.");

        Number = number;

        int div = (int)Math.Log10(Number) - 1;
        Year = (Number / (int)Math.Pow(10, div)) % 10;
    }

    public int Number { get; }
    public int Year { get; }

    public override string ToString() => $"{_prefix}{Number}";

    public override bool Equals(object? obj)
    {
        if ((obj is null) || !(obj is GroupName))
            return false;
        return Number == ((GroupName)obj).Number;
    }

    public override int GetHashCode() => ((int)_prefix << 24) | Number;
}