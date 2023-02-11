# Лабораторная 1. Shops

### Цель
Продемонстрировать умение выделять сущности и проектировать по ним классы.

### Прикладная область
Магазин, покупатель, доставка, пополнение и покупка товаров. Магазин имеет уникальный идентификатор, название (не обязательно уникальное) и адрес. В каждом магазине установлена своя цена на товар и есть в наличии некоторое количество единиц товара (какого-то товара может и не быть вовсе). Покупатель может производить покупку. Во время покупки - он передает нужную сумму денег магазину. Поставка товаров представляет собой набор товаров, их цен и количества, которые должны быть добавлены в магазин.

### Тест кейсы
- Поставка товаров в магазин. Создаётся магазин, добавляются в систему товары, происходит поставка товаров в магазин. После добавления товары можно купить.
- Установка и изменение цен на какой-то товар в магазине.
- Поиск магазина, в котором набор товаров можно купить максимально дешево. Обработать ситуации, когда товара может быть недостаточно или товаров может небыть нигде.
- Покупка партии товаров в магазине (набор пар товар + количество). Нужно убедиться, что товаров хватает, что у пользователя достаточно денег. После покупки должны передаваться деньги, а количество товаров измениться.

### NB:
Можно не поддерживать разные цены для одного магазина. Как вариант, можно брать старую цену, если магазин уже содержит этот товар. Иначе брать цену указанную в поставке.

Пример ожидаемого формата тестов представлен ниже.<br>
**Используемые в тестах API магазина/менеджера/etc не являются интерфейсом для реализации в данной лабораторной. Не нужно ему следовать 1 в 1, это просто пример.**

```csharp
public void SomeTest(moneyBefore, productPrice, productCount, productToBuyCount)
{
    var person = new Person("name", moneyBefore);
    var shopManager = new ShopManager();
    var shop = shopManager.Create("shop name", ...);
    var product = shopManager.RegisterProduct("product name");

    shop.AddProducts( ... );
    shop.Buy(person, ...);

    Assert.AreEquals(moneyBefore - productPrice  * productToBuyCount, person.Money);
    Assert.AreEquals(productCount - productToBuyCount , shop.GetProductInfo(product).Count);
}
```