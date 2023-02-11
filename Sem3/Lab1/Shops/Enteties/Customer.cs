using Shops.Exceptions;

namespace Shops.Enteties;

public class Customer
{
    public const decimal MinBallance = 0;
    public const decimal MinSpendAmount = 0;

    public Customer(string name, decimal ballance)
    {
        if (string.IsNullOrEmpty(name))
            throw new FormatException("Name can't be empty");

        if (ballance < MinBallance)
            throw new ShopsNegativeValueException($"Ballance can't be less than {MinBallance}");

        (Name, Ballance) = (name, ballance);
    }

    public string Name { get; }
    public decimal Ballance { get; private set; }

    public void Spend(decimal amount)
    {
        if (amount < MinSpendAmount)
            throw new ShopsNegativeValueException($"Can't spend less than {MinSpendAmount}.");

        if (Ballance - amount < MinBallance)
            throw new ShopsTooPoorException($"Can't spend {amount} out of {Ballance}, would leave less than {MinBallance} left.");

        Ballance -= amount;
    }
}