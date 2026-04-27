using GoodHamburger.Domain.Menu;

namespace GoodHamburger.Domain.Tests;

public class MenuCatalogTests
{
    [Fact]
    public void All_should_return_five_items()
    {
        var items = MenuCatalog.All;

        Assert.Equal(5, items.Count);
        Assert.Contains(items, x => x.Code == MenuItemCode.XBurger);
        Assert.Contains(items, x => x.Code == MenuItemCode.XEgg);
        Assert.Contains(items, x => x.Code == MenuItemCode.XBacon);
        Assert.Contains(items, x => x.Code == MenuItemCode.Fries);
        Assert.Contains(items, x => x.Code == MenuItemCode.Soda);
    }

    [Theory]
    [InlineData("XBurger", MenuItemCode.XBurger)]
    [InlineData("x burger", MenuItemCode.XBurger)]
    [InlineData("X Egg", MenuItemCode.XEgg)]
    [InlineData("batata frita", MenuItemCode.Fries)]
    [InlineData("refrigerante", MenuItemCode.Soda)]
    public void TryParse_should_accept_code_and_name_variations(string input, MenuItemCode expected)
    {
        var ok = MenuCatalog.TryParse(input, out var code);

        Assert.True(ok);
        Assert.Equal(expected, code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-found")]
    public void TryParse_should_reject_invalid_values(string input)
    {
        var ok = MenuCatalog.TryParse(input, out _);

        Assert.False(ok);
    }
}