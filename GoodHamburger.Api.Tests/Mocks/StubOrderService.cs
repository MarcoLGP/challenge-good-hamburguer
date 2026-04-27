using GoodHamburger.Application.Contracts;
using GoodHamburger.Application.Services;
using GoodHamburger.Application.Shared;

namespace GoodHamburger.Api.Tests.Mocks;

internal sealed class StubOrderService : IOrderService
{
    private readonly Dictionary<Guid, OrderDto> _orders = new();

    public Task<Result<OrderDto>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();

        var dto = new OrderDto(
            id,
            [
                new MenuItemDto("XBurger", "X Burger", "Sandwich", 5.00m)
            ],
            5.00m,
            0.00m,
            0.00m,
            5.00m,
            DateTimeOffset.Parse("2026-04-22T10:00:00Z"),
            DateTimeOffset.Parse("2026-04-22T10:00:00Z"));

        _orders[id] = dto;
        return Task.FromResult(Result<OrderDto>.Success(dto));
    }

    public Task<Result<OrderDto>> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (!_orders.TryGetValue(id, out var dto))
        {
            return Task.FromResult(Result<OrderDto>.Failure(new ApplicationError("order_not_found", "Pedido não encontrado.")));
        }

        return Task.FromResult(Result<OrderDto>.Success(dto));
    }

    public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_orders.ContainsKey(id))
        {
            return Task.FromResult(Result.Failure(new ApplicationError("order_not_found", "Pedido não encontrado.")));
        }

        _orders.Remove(id);
        return Task.FromResult(Result.Success());
    }

    public Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_orders.TryGetValue(id, out var dto))
        {
            return Task.FromResult(Result<OrderDto>.Failure(new ApplicationError("order_not_found", "Pedido não encontrado.")));
        }

        return Task.FromResult(Result<OrderDto>.Success(dto));
    }

    public Task<Result<IReadOnlyList<OrderDto>>> GetAllAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Result<IReadOnlyList<OrderDto>>.Success(_orders.Values.ToList()));
}