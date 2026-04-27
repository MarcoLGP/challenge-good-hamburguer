using GoodHamburger.Domain.Menu;

namespace GoodHamburger.Domain.Orders;

public sealed record OrderPricing(
    decimal Subtotal,
    decimal DiscountRate,
    decimal Discount,
    decimal Total);

public static class OrderPricingCalculator
{
    public static OrderPricing Calculate(IReadOnlyCollection<MenuItemDefinition> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var subtotal = Round(items.Sum(item => item.Price));

        var hasSandwich = items.Any(item => item.Category == MenuItemCategory.Sandwich);
        var hasSide = items.Any(item => item.Category == MenuItemCategory.Side);
        var hasDrink = items.Any(item => item.Category == MenuItemCategory.Drink);

        var discountRate =
            hasSandwich && hasSide && hasDrink ? 0.20m :
            hasSandwich && hasDrink ? 0.15m :
            hasSandwich && hasSide ? 0.10m :
            0m;

        var discount = Round(subtotal * discountRate);
        var total = Round(subtotal - discount);

        return new OrderPricing(subtotal, discountRate, discount, total);
    }

    private static decimal Round(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
