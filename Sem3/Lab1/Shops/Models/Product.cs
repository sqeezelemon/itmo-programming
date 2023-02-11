namespace Shops.Models;

public class Product
{
    public Product(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new FormatException("Name can't be empty.");

        Name = name;
    }

    public string Name { get; }

    public override bool Equals(object? obj)
    {
        if ((obj is null) || !(obj is Product))
            return false;
        return ((Product)obj).Name.Equals(Name);
    }

    public override int GetHashCode() => Name.GetHashCode();
}