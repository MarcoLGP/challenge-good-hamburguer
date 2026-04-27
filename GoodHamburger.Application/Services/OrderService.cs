using GoodHamburger.Application.Abstractions.Persistence;
using GoodHamburger.Application.Contracts;
using GoodHamburger.Application.Shared;
using GoodHamburger.Domain.Common;
using GoodHamburger.Domain.Menu;
using GoodHamburger.Domain.Orders;

namespace GoodHamburger.Application.Services;

public interface IOrderService
{
    Task<Result<OrderDto>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

    Task<Result<OrderDto>> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<OrderDto>>> GetAllAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
}

public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public OrderService(IOrderRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<OrderDto>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var parseResult = ParseItemCodes(request?.ItemCodes);
        if (!parseResult.IsSuccess)
        {
            return Result<OrderDto>.Failure(parseResult.Errors.ToArray());
        }

        try
        {
            var order = Order.Create(Guid.NewGuid(), parseResult.Value!, _timeProvider.GetUtcNow());
            await _repository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<OrderDto>.Success(Map(order));
        }
        catch (DomainException ex)
        {
            return Result<OrderDto>.Failure(new ApplicationError(ex.Code, ex.Message));
        }
    }

    public async Task<Result<OrderDto>> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Result<OrderDto>.Failure(new ApplicationError("invalid_order_id", "O identificador do pedido é inválido."));
        }

        var parseResult = ParseItemCodes(request?.ItemCodes);
        if (!parseResult.IsSuccess)
        {
            return Result<OrderDto>.Failure(parseResult.Errors.ToArray());
        }

        var order = await _repository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure(new ApplicationError("order_not_found", "Pedido não encontrado."));
        }

        try
        {
            order.ReplaceItems(parseResult.Value!, _timeProvider.GetUtcNow());
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<OrderDto>.Success(Map(order));
        }
        catch (DomainException ex)
        {
            return Result<OrderDto>.Failure(new ApplicationError(ex.Code, ex.Message));
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure(new ApplicationError("invalid_order_id", "O identificador do pedido é inválido."));
        }

        var order = await _repository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return Result.Failure(new ApplicationError("order_not_found", "Pedido não encontrado."));
        }

        _repository.Remove(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Result<OrderDto>.Failure(new ApplicationError("invalid_order_id", "O identificador do pedido é inválido."));
        }

        var order = await _repository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.Failure(new ApplicationError("order_not_found", "Pedido não encontrado."));
        }

        return Result<OrderDto>.Success(Map(order));
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> GetAllAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var orders = await _repository.GetAllAsync(searchTerm, cancellationToken);
        var dtos = orders.Select(Map).ToArray();

        return Result<IReadOnlyList<OrderDto>>.Success(dtos);
    }

    private static Result<IReadOnlyCollection<MenuItemCode>> ParseItemCodes(IReadOnlyCollection<string>? codes)
    {
        if (codes is null || codes.Count == 0)
        {
            return Result<IReadOnlyCollection<MenuItemCode>>.Failure(
                new ApplicationError("invalid_order", "O pedido precisa conter ao menos um item."));
        }

        var parsed = new List<MenuItemCode>(codes.Count);
        var invalidCodes = new List<string>();

        foreach (var code in codes)
        {
            if (MenuCatalog.TryParse(code, out var itemCode))
            {
                parsed.Add(itemCode);
            }
            else
            {
                invalidCodes.Add(code);
            }
        }

        if (invalidCodes.Count > 0)
        {
            return Result<IReadOnlyCollection<MenuItemCode>>.Failure(
                new ApplicationError("menu_item_not_found", $"Item inválido no pedido: {string.Join(", ", invalidCodes)}."));
        }

        return Result<IReadOnlyCollection<MenuItemCode>>.Success(parsed);
    }

    private static OrderDto Map(Order order)
    {
        var items = order.GetItems()
            .Select(item => new MenuItemDto(
                item.Code.ToString(),
                item.Name,
                item.Category.ToString(),
                item.Price))
            .ToArray();

        return new OrderDto(
            order.Id,
            items,
            order.Subtotal,
            order.DiscountRate,
            order.Discount,
            order.Total,
            order.CreatedAt,
            order.UpdatedAt);
    }
}