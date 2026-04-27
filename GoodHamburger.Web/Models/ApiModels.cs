namespace GoodHamburger.Web.Models;

/// <summary>Item do cardápio retornado pela API.</summary>
public sealed record MenuItemDto(
    string Code,
    string Name,
    string Category,
    decimal Price);

/// <summary>Pedido completo retornado pela API.</summary>
public sealed record OrderDto(
    Guid Id,
    List<MenuItemDto> Items,
    decimal Subtotal,
    decimal DiscountRate,
    decimal Discount,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>Corpo da requisição POST /api/orders.</summary>
public sealed record CreateOrderRequest(List<string> ItemCodes);

/// <summary>Corpo da requisição PUT /api/orders/{id}.</summary>
public sealed record UpdateOrderRequest(List<string> ItemCodes);

/// <summary>Resposta de erro no padrão RFC 7807 / ProblemDetails.</summary>
public sealed record ApiProblemDetails(
    int? Status,
    string? Title,
    string? Detail,
    string? Instance,
    string? Code);

/// <summary>Resultado tipado de operações da API.</summary>
public sealed record ApiResult<T>(T? Value, string? Error)
{
    public bool IsSuccess => Error is null;

    public static ApiResult<T> Ok(T value) => new(value, null);
    public static ApiResult<T> Fail(string error) => new(default, error);
}
