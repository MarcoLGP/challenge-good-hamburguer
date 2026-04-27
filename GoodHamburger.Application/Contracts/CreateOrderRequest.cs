namespace GoodHamburger.Application.Contracts;

public sealed record CreateOrderRequest(IReadOnlyCollection<string> ItemCodes);
