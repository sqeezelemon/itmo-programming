using Shops.Enteties;
using Shops.Exceptions;
using Shops.Models;
using Shops.Services;
using Xunit;

namespace Shops.Test;

public class ShopManagerTest
{
    private ShopManager _manager = new ShopManager();

    private Address _address = new Address("Биробиджан");

    [Fact]
    public void Shipment_AcceptedAndItemsAvailableForPurchase()
    {
        Shop shop = _manager.Create("Немагазин Downgrade", _address);
        Product product = _manager.Register("Несвежее Немолоко");
        Customer customer = new Customer("Неконстантин Неконстантинопольский", 1000M);

        Container container1 = new Container(product, 10M, 50);
        shop.AcceptShipment(container1);

        // Ballance - 10*10
        var exception = Record.Exception(() => shop.Buy(customer, product, 10));
        Assert.Null(exception);

        // Only 40 left
        Assert.Throws<ShopsNotEnoughInventoryException>(() => shop.Buy(customer, product, 50));

        Container container2 = new Container(product, 16M, 60);
        shop.AcceptShipment(container2, true);

        // Ballance - 16*50
        exception = Record.Exception(() => shop.Buy(customer, product, 50));
        Assert.Null(exception);

        // Costs 150, has 100
        Assert.Throws<ShopsTooPoorException>(() => shop.Buy(customer, product, 10));
    }

    [Fact]
    public void Shop_SetPriceAndPriceChanged()
    {
        Shop shop = _manager.Create("Азбука Вкусвилла", _address);
        Product product = _manager.Register("Блин с ничем");

        Container container = new Container(product, 100M, 10);
        shop.AcceptShipment(container);

        Assert.Equal(container.Price, shop.GetPrice(product));

        shop.SetPrice(product, 90M);
        Assert.Equal(90M, shop.GetPrice(product));

        container = new Container(product, 50M, 10);
        shop.AcceptShipment(container, true);
        Assert.Equal(container.Price, shop.GetPrice(product));

        Assert.Throws<ShopsNegativeValueException>(() => shop.SetPrice(product, -1));
    }

    [Fact]
    public void Shop_FindBestDeal()
    {
        Shop shop1 = _manager.Create("Столовая номер 5", _address);
        Shop shop2 = _manager.Create("Ашан", _address);
        Shop shop3 = _manager.Create("Яндекс Еда", _address);
        Product product = _manager.Register("Папперделле с Лососем");

        // Cheap but little stock
        Container container1 = new Container(product, 1M, 10);
        shop1.AcceptShipment(container1);

        // Expensive but a lot of stock
        Container container2 = new Container(product, 10M, 100);
        shop2.AcceptShipment(container2);

        // Worst option in all cases
        Container container3 = new Container(product, 100M, 1000);
        shop3.AcceptShipment(container3);

        Assert.Equal(shop1, _manager.FindBestDeal(product, 9));
        Assert.Equal(shop2, _manager.FindBestDeal(product, 15));
        Assert.Equal(shop3, _manager.FindBestDeal(product, 1000));
        Assert.Null(_manager.FindBestDeal(product, 2000));
    }

    [Fact]
    public void Shop_BuyInBatch()
    {
        Shop shop = _manager.Create("Мандекс Яркет", _address);

        Product product1 = _manager.Register("Надувной бассейн ростест");
        Container container1 = new Container(product1, 10M, 10);
        shop.AcceptShipment(container1);

        Product product2 = _manager.Register("Холодильник СыктывкарХолодПром 30");
        Container container2 = new Container(product2, 20M, 20);
        shop.AcceptShipment(container2);

        Product product3 = _manager.Register("Аквариум с функцией электрочайника");
        Container container3 = new Container(product3, 30M, 30);
        shop.AcceptShipment(container3);

        Customer customer = new Customer("Пользователь 20 уровня", 1000);

        // Alright order
        List<Order> order1 = new List<Order>
        {
            new Order(product1, 10),
            new Order(product2, 10),
            new Order(product3, 10),
        };
        var exception = Record.Exception(() => shop.Buy(customer, order1));
        Assert.Null(exception);

        decimal moneyBeforeOrder2 = customer.Ballance;

        List<Order> order2 = new List<Order>
        {
            new Order(product2, 1),
            new Order(product3, 1),
            new Order(product1, 1),
        };

        // Not enough product1;
        Assert.Throws<ShopsNotEnoughInventoryException>(() => shop.Buy(customer, order2));

        // Check that customer wasn't charged
        Assert.Equal(customer.Ballance, moneyBeforeOrder2);

        List<Order> order3 = new List<Order>
        {
            new Order(product2, 9),
            new Order(product3, 19),
        };

        decimal moneyBeforeOrder3 = customer.Ballance;

        // Not enough money.
        Assert.Throws<ShopsTooPoorException>(() => shop.Buy(customer, order3));

        // Check that customer wasn't charged
        Assert.Equal(customer.Ballance, moneyBeforeOrder3);
    }
}