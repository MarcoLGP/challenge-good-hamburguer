using GoodHamburger.Application.Contracts;
using GoodHamburger.Application.Services;

namespace GoodHamburger.Api.Tests.Mocks;

internal sealed class StubMenuService : IMenuService
{
    private readonly IReadOnlyList<MenuItemDto> _menu;

    public StubMenuService()
    {
        _menu =
        [
            new MenuItemDto("XBurger", "X Burger", "Sandwich", 5.00m),
            new MenuItemDto("Fries", "Batata frita", "Side", 2.00m),
            new MenuItemDto("Soda", "Refrigerante", "Drink", 2.50m),
        ];
    }

    public IReadOnlyList<MenuItemDto> GetMenu() => _menu;
}