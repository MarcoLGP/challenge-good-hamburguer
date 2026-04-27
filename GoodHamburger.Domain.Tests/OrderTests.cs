using GoodHamburger.Domain.Menu;
using GoodHamburger.Domain.Orders;

namespace GoodHamburger.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void Create_should_build_order_and_calculate_totals()
    {
        var now = DateTimeOffset.Parse("2026-04-22T10:00:00Z");

        var order = Order.Create(
            Guid.NewGuid(),
            new[] { MenuItemCode.XBurger, MenuItemCode.Fries, MenuItemCode.Soda },
            now);

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(5.00m, order.GetItems().First(x => x.Code == MenuItemCode.XBurger).Price);
        Assert.Equal(9.50m, order.Subtotal);
        Assert.Equal(0.20m, order.DiscountRate);
        Assert.Equal(1.90m, order.Discount);
        Assert.Equal(7.60m, order.Total);
        Assert.Equal(now, order.CreatedAt);
        Assert.Equal(now, order.UpdatedAt);
    }

    [Fact]
    public void Create_should_reject_duplicate_sandwich()
    {
        var now = DateTimeOffset.UtcNow;

        var ex = Assert.Throws<DuplicateCategoryException>(() =>
            Order.Create(Guid.NewGuid(), new[] { MenuItemCode.XBurger, MenuItemCode.XEgg }, now));

        Assert.Equal("duplicated_item", ex.Code);
    }

    [Fact]
    public void Create_should_reject_empty_order()
    {
        var now = DateTimeOffset.UtcNow;

        var ex = Assert.Throws<InvalidOrderException>(() =>
            Order.Create(Guid.NewGuid(), Array.Empty<MenuItemCode>(), now));

        Assert.Equal("invalid_order", ex.Code);
    }

    [Fact]
    public void ReplaceItems_should_update_pricing_and_updated_at()
    {
        var createdAt = DateTimeOffset.Parse("2026-04-22T10:00:00Z");
        var later = createdAt.AddMinutes(10);

        var order = Order.Create(Guid.NewGuid(), new[] { MenuItemCode.XBurger }, createdAt);

        order.ReplaceItems(new[] { MenuItemCode.XBurger, MenuItemCode.Soda }, later);

        Assert.Equal(7.50m, order.Subtotal);
        Assert.Equal(0.15m, order.DiscountRate);
        Assert.Equal(1.13m, order.Discount);
        Assert.Equal(6.37m, order.Total);
        Assert.Equal(later, order.UpdatedAt);
    }
}