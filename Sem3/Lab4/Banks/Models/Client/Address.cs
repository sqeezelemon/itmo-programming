using Banks.Exceptions;
namespace Banks.Models;

public class Address
{
    public Address(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BanksInvalidValueException("Can't infer address from an empty string");
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override bool Equals(object? obj)
    {
        if (obj is Address addr)
            return addr.Value.Equals(Value);
        return false;
    }
}