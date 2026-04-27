using GoodHamburger.Application.Contracts;
using GoodHamburger.Domain.Menu;

namespace GoodHamburger.Application.Services;

public interface IMenuService
{
    IReadOnlyList<MenuItemDto> GetMenu();
}

public sealed class MenuService : IMenuService
{
    public IReadOnlyList<MenuItemDto> GetMenu()
    {
        return MenuCatalog.All
            .Select(item => new MenuItemDto(
                item.Code.ToString(),
                item.Name,
                item.Category.ToString(),
                item.Price))
            .ToArray();
    }
}