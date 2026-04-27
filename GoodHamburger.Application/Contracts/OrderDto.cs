namespace GoodHamburger.Application.Contracts;

public sealed record OrderDto(
    Guid Id,
    IReadOnlyCollection<MenuItemDto> Items,
    decimal Subtotal,
    decimal DiscountRate,
    decimal Discount,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);