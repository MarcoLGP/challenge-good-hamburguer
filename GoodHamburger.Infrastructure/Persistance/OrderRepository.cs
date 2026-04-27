using GoodHamburger.Application.Abstractions.Persistence;
using GoodHamburger.Domain.Menu;
using GoodHamburger.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Persistence;

internal sealed class OrderRepository(GoodHamburgerDbContext dbContext) : IOrderRepository
{
    private readonly GoodHamburgerDbContext _dbContext = dbContext;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetAllAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        // Carrega todos os pedidos para memória para evitar problemas de tradução do LINQ (especialmente no SQLite)
        var allOrders = await _dbContext.Orders.AsNoTracking().ToListAsync(cancellationToken);
        
        var term = (searchTerm ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(term))
        {
            return allOrders.OrderByDescending(x => x.CreatedAt).ToList();
        }

        // 1. Filtro por ID exato
        if (Guid.TryParse(term, out var targetId))
        {
            return allOrders.Where(x => x.Id == targetId).ToList();
        }

        // 2. Filtro por nome de item ou código do item
        return allOrders
            .Where(o => o.GetItems().Any(i => 
                i.Name.ToLowerInvariant().Contains(term) || 
                i.Code.ToString().ToLowerInvariant().Contains(term)))
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => _dbContext.Orders.AddAsync(order, cancellationToken).AsTask();

    public void Remove(Order order)
        => _dbContext.Orders.Remove(order);
}