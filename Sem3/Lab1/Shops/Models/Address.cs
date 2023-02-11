namespace Shops.Models;

public class Address
{
    public Address(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new FormatException("Address can't be empty");

        Value = address;
    }

    public string Value { get; }

    public override string ToString() => Value;

    public override int GetHashCode() => Value.GetHashCode();
}