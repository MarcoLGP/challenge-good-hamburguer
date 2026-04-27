using GoodHamburger.Application.Abstractions.Persistence;
using GoodHamburger.Domain.Orders;

namespace GoodHamburger.Application.Tests.Mocks;

internal sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_orders.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyList<Order>> GetAllAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var result = _orders.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLowerInvariant();
            result = result.Where(o => 
                o.Id.ToString().Contains(term) || 
                o.GetItems().Any(i => i.Name.ToLowerInvariant().Contains(term) || i.Code.ToString().ToLowerInvariant().Contains(term)));
        }
        return Task.FromResult<IReadOnlyList<Order>>(result.ToList());
    }

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _orders.Add(order);
        return Task.CompletedTask;
    }

    public void Remove(Order order)
    {
        _orders.Remove(order);
    }

    public IReadOnlyList<Order> Snapshot() => _orders.ToList();
}