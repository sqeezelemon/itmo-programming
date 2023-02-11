using System.Linq;
using Shops.Exceptions;

namespace Shops.Models;

public class Container
{
    public const int MinQuantity = 0;
    public const decimal MinPrice = 0;

    public Container(Product product, decimal price, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (quantity < MinQuantity)
            throw new ShopsNegativeValueException($"Quanity can't be less than {MinQuantity} ({quantity} supplied).");
        if (price < MinPrice)
            throw new ShopsNegativeValueException($"Price can't be less than {MinPrice} ({price} supplied).");

        (Product, Price, Quantity) = (product, price, quantity);
    }

    public Product Product { get; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }

    public void ChangeQuantity(int amount)
    {
        if (Quantity + amount < MinQuantity)
            throw new ShopsNegativeValueException($"Decreasing quantity {Quantity} by {amount} would cause negative quantity.");

        Quantity += amount;
    }

    public void ChangePrice(decimal amount)
    {
        if (Price + amount < MinPrice)
            throw new ShopsNegativeValueException($"Changing price {Price} by {amount} would cause negative price.");

        Price += amount;
    }

    public void SetQuantity(int value)
    {
        if (value < MinQuantity)
            throw new ShopsNegativeValueException($"Can't set negative quantity.");
        Quantity = value;
    }

    public void SetPrice(decimal value)
    {
        if (value < MinPrice)
            throw new ShopsNegativeValueException($"Can't set negative quantity.");
        Price = value;
    }

    public override bool Equals(object? obj)
    {
        if ((obj is null) || !(obj is Product))
            return false;
        return ((Container)obj).Product.Equals(Product);
    }

    public override int GetHashCode() => Product.GetHashCode();

    public Container Copy() => (Container)this.MemberwiseClone();
}