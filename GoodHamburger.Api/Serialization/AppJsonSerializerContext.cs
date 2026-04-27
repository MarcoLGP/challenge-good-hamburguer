using System.Text.Json.Serialization;
using GoodHamburger.Api.Endpoints;
using GoodHamburger.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.Api.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
[JsonSerializable(typeof(CreateOrderRequest))]
[JsonSerializable(typeof(UpdateOrderRequest))]
[JsonSerializable(typeof(MenuItemDto))]
[JsonSerializable(typeof(List<MenuItemDto>))]
[JsonSerializable(typeof(IReadOnlyList<MenuItemDto>))]
[JsonSerializable(typeof(OrderDto))]
[JsonSerializable(typeof(List<OrderDto>))]
[JsonSerializable(typeof(IReadOnlyList<OrderDto>))]
[JsonSerializable(typeof(RegisterRequest))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(RefreshTokenRequest))]
[JsonSerializable(typeof(LogoutRequest))]
[JsonSerializable(typeof(AuthResponse))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(IEnumerable<ProblemDetails>))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
