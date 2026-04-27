using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Menu;

namespace GoodHamburger.Domain.Orders;

public sealed class Order
{
    private Order()
    {
    }

    private Order(Guid id, DateTimeOffset createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public MenuItemCode? SandwichCode { get; private set; }

    public MenuItemCode? SideCode { get; private set; }

    public MenuItemCode? DrinkCode { get; private set; }

    public decimal Subtotal { get; private set; }

    public decimal DiscountRate { get; private set; }

    public decimal Discount { get; private set; }

    public decimal Total { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Order Create(Guid id, IEnumerable<MenuItemCode> itemCodes, DateTimeOffset utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidOrderException("O identificador do pedido é inválido.");
        }

        var order = new Order(id, utcNow);
        order.ReplaceItems(itemCodes, utcNow);
        return order;
    }

    public void ReplaceItems(IEnumerable<MenuItemCode> itemCodes, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(itemCodes);

        var codes = itemCodes.ToArray();
        if (codes.Length == 0)
        {
            throw new InvalidOrderException("O pedido precisa conter ao menos um item.");
        }

        var definitions = MenuCatalog.ResolveItems(codes);

        var duplicatedCategory = definitions
            .GroupBy(item => item.Category)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicatedCategory is not null)
        {
            throw new DuplicateCategoryException(duplicatedCategory.Key);
        }

        SandwichCode = definitions.FirstOrDefault(item => item.Category == MenuItemCategory.Sandwich)?.Code;
        SideCode = definitions.FirstOrDefault(item => item.Category == MenuItemCategory.Side)?.Code;
        DrinkCode = definitions.FirstOrDefault(item => item.Category == MenuItemCategory.Drink)?.Code;

        var pricing = OrderPricingCalculator.Calculate(definitions);
        Subtotal = pricing.Subtotal;
        DiscountRate = pricing.DiscountRate;
        Discount = pricing.Discount;
        Total = pricing.Total;

        UpdatedAt = utcNow;
    }

    public IReadOnlyCollection<MenuItemDefinition> GetItems()
    {
        var items = new List<MenuItemDefinition>(3);

        if (SandwichCode is { } sandwichCode)
        {
            items.Add(MenuCatalog.GetDefinition(sandwichCode));
        }

        if (SideCode is { } sideCode)
        {
            items.Add(MenuCatalog.GetDefinition(sideCode));
        }

        if (DrinkCode is { } drinkCode)
        {
            items.Add(MenuCatalog.GetDefinition(drinkCode));
        }

        return items;
    }
}

public sealed class DuplicateCategoryException(MenuItemCategory category) : DomainException("duplicated_item", category switch
        {
            MenuItemCategory.Sandwich => "Cada pedido pode conter apenas um sanduíche.",
            MenuItemCategory.Side => "Cada pedido pode conter apenas uma batata frita.",
            MenuItemCategory.Drink => "Cada pedido pode conter apenas um refrigerante.",
            _ => "Cada pedido pode conter apenas um item por categoria."
        })
{
    public MenuItemCategory Category { get; } = category;
}

public sealed class InvalidOrderException : DomainException
{
    public InvalidOrderException(string message)
        : base("invalid_order", message)
    {
    }
}