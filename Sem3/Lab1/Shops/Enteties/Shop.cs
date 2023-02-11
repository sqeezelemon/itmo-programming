using Shops.Exceptions;
using Shops.Models;

namespace Shops.Enteties;

public class Shop
{
    private List<Container> _inventory = new List<Container>();

    public Shop(int id, string name, Address address)
    {
        ArgumentNullException.ThrowIfNull(address);

        if (string.IsNullOrEmpty(name))
            throw new FormatException("Can't create shop with a null value");

        (Id, Name, Address) = (id, name, address);
    }

    public int Id { get; }
    public string Name { get; }
    public Address Address { get; }

    public bool HasProduct(Product product) => _inventory.Any(c => c.Product == product);

    public int ProductQuantity(Product product) => FindContainer(product)?.Quantity ?? 0;

    public void SetPrice(Product product, decimal newPrice)
    {
        ArgumentNullException.ThrowIfNull(product);
        Container? container = FindContainer(product);
        if (container is null)
            throw new ShopsNotFoundException($"Price for product {product} not found.");
        container!.SetPrice(newPrice);
    }

    public decimal? FindPrice(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        Container? container = FindContainer(product);
        return container?.Price;
    }

    public decimal GetPrice(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        Container? container = FindContainer(product);
        if (container is null)
            throw new ShopsNotFoundException($"Price for product {product} not found.");
        return container!.Price;
    }

    public decimal GetQuantity(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        Container? container = FindContainer(product);
        if (container is null)
            throw new ShopsNotFoundException($"Product {product} not found.");

        return container!.Quantity;
    }

    public void AcceptShipment(Container shipment, bool changePrice = false)
    {
        Container? container = FindContainer(shipment.Product);
        if (container is null)
        {
            Container containerCopy = shipment.Copy();
            container = containerCopy;
            _inventory.Add(container!);
            return;
        }

        container!.ChangeQuantity(shipment.Quantity);
        if (changePrice)
            container!.SetPrice(shipment.Price);
    }

    public void Buy(Customer customer, Product product, int quantity)
    {
        if (quantity < 0)
            throw new ShopsNegativeValueException("Quantity can't be null");

        Container? container = FindContainer(product);
        if (container is null)
            throw new ShopsNotEnoughInventoryException($"Product {product} not found");

        if (container!.Quantity < quantity)
            throw new ShopsNotEnoughInventoryException($"Can't buy {quantity} items, only ${container!.Quantity} in stock.");

        customer.Spend(container!.Price * (decimal)quantity);
        container!.ChangeQuantity(-quantity);
    }

    public void Buy(Customer customer, List<Order> orderList)
    {
        List<Order> orders =
            orderList.GroupBy(o => o.Product)
            .Select(g => new Order(g.Key, g.Sum(o => o.Quantity)))
            .ToList();

        decimal total = 0;

        foreach (var order in orders)
        {
            ArgumentNullException.ThrowIfNull(order);

            Container? container = FindContainer(order.Product);
            if (container is null)
                throw new ShopsNotFoundException($"Product {order.Product} not found.");

            if (container.Quantity < order.Quantity)
                throw new ShopsNotEnoughInventoryException($"Can't buy {order.Quantity} items, only ${container!.Quantity} in stock.");

            total += container.Price * (decimal)order.Quantity;
        }

        customer.Spend(total);

        foreach (var order in orders)
        {
            ArgumentNullException.ThrowIfNull(order);

            Container? container = FindContainer(order.Product);
            if (container is null)
                throw new ShopsNotFoundException($"Product {order.Product} not found.");

            container!.ChangeQuantity(-order.Quantity);
        }
    }

    public override int GetHashCode() => Id.GetHashCode();

    public override bool Equals(object? obj)
    {
        if ((obj is null) || !(obj is Shop))
            return false;
        return ((Shop)obj).Id.Equals(Id);
    }

    public override string ToString() => $"{Name} (ID: {Id})";

    private Container? FindContainer(Product product) => _inventory.SingleOrDefault(c => c.Product == product);
}