using GoodHamburger.Domain.Menu;
using GoodHamburger.Domain.Orders;
using Xunit;

namespace GoodHamburger.Domain.Tests;

public class OrderPricingCalculatorTests
{
    [Fact]
    public void Should_apply_20_percent_discount_for_sandwich_side_drink()
    {
        var items = new[]
        {
            MenuCatalog.GetDefinition(MenuItemCode.XBurger),
            MenuCatalog.GetDefinition(MenuItemCode.Fries),
            MenuCatalog.GetDefinition(MenuItemCode.Soda),
        };

        var pricing = OrderPricingCalculator.Calculate(items);

        Assert.Equal(9.50m, pricing.Subtotal);
        Assert.Equal(0.20m, pricing.DiscountRate);
        Assert.Equal(1.90m, pricing.Discount);
        Assert.Equal(7.60m, pricing.Total);
    }

    [Fact]
    public void Should_apply_15_percent_discount_for_sandwich_and_drink()
    {
        var items = new[]
        {
            MenuCatalog.GetDefinition(MenuItemCode.XEgg),
            MenuCatalog.GetDefinition(MenuItemCode.Soda),
        };

        var pricing = OrderPricingCalculator.Calculate(items);

        Assert.Equal(7.00m, pricing.Subtotal);
        Assert.Equal(0.15m, pricing.DiscountRate);
        Assert.Equal(1.05m, pricing.Discount);
        Assert.Equal(5.95m, pricing.Total);
    }

    [Fact]
    public void Should_apply_10_percent_discount_for_sandwich_and_side()
    {
        var items = new[]
        {
            MenuCatalog.GetDefinition(MenuItemCode.XBacon),
            MenuCatalog.GetDefinition(MenuItemCode.Fries),
        };

        var pricing = OrderPricingCalculator.Calculate(items);

        Assert.Equal(9.00m, pricing.Subtotal);
        Assert.Equal(0.10m, pricing.DiscountRate);
        Assert.Equal(0.90m, pricing.Discount);
        Assert.Equal(8.10m, pricing.Total);
    }

    [Fact]
    public void Should_not_apply_discount_for_sandwich_only()
    {
        var items = new[]
        {
            MenuCatalog.GetDefinition(MenuItemCode.XBurger),
        };

        var pricing = OrderPricingCalculator.Calculate(items);

        Assert.Equal(5.00m, pricing.Subtotal);
        Assert.Equal(0.00m, pricing.DiscountRate);
        Assert.Equal(0.00m, pricing.Discount);
        Assert.Equal(5.00m, pricing.Total);
    }
}