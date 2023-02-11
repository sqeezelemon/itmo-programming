using Shops.Exceptions;

namespace Shops.Models;

public class Order
{
    public const int MinQuantity = 1;

    public Order(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (quantity < MinQuantity)
            throw new ShopsNegativeValueException($"Quantity too small (min {MinQuantity} ).");

        (Product, Quantity) = (product, quantity);
    }

    public Product Product { get; }
    public int Quantity { get; }
}