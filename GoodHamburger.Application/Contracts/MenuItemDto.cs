namespace GoodHamburger.Application.Contracts;

public sealed record MenuItemDto(
    string Code,
    string Name,
    string Category,
    decimal Price);