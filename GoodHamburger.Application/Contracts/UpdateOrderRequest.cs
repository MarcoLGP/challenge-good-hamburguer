namespace GoodHamburger.Application.Contracts;

public sealed record UpdateOrderRequest(IReadOnlyCollection<string> ItemCodes);
