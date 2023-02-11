using Shops.Enteties;
using Shops.Exceptions;
using Shops.Models;

namespace Shops.Services;

public class ShopManager
{
    private List<Shop> _shops = new List<Shop>();
    private HashSet<Product> _products = new HashSet<Product>();
    private int _nextShopId = 0;

    public ShopManager() { }

    public Product Register(string productName)
    {
        var product = new Product(productName);
        if (_products.Contains(product))
            throw new ShopsDuplicateException($"Product with name {productName} already exists!");

        _products.Add(product);
        return product;
    }

    public Product? FindProduct(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new FormatException("Product name can't be null");
        return _products.SingleOrDefault(p => p.Name == name);
    }

    public bool ProductIsRegistered(Product product)
    {
        return _products.Contains(product);
    }

    public Shop Create(string shopName, Address address)
    {
        Shop shop = new Shop(_nextShopId, shopName, address);
        _shops.Add(shop);
        ++_nextShopId;
        return shop;
    }

    public Shop? FindBestDeal(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);

        Shop? shop = _shops
            .Where(s => s.HasProduct(product))
            .Where(s => s.GetQuantity(product) >= quantity)
            .MinBy(s => s.GetPrice(product));

        return shop;
    }
}