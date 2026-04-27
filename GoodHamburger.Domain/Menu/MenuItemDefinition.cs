namespace GoodHamburger.Domain.Menu
{
    public sealed record MenuItemDefinition(
    MenuItemCode Code,
    string Name,
    MenuItemCategory Category,
    decimal Price);
}
