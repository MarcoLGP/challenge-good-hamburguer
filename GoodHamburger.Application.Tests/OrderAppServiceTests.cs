using GoodHamburger.Application.Contracts;
using GoodHamburger.Application.Services;
using GoodHamburger.Application.Tests.Mocks;

namespace GoodHamburger.Application.Tests;

public class OrderAppServiceTests
{
    private static OrderService CreateSut(
        InMemoryOrderRepository repository,
        MockUnitOfWork unitOfWork,
        TimeProvider? timeProvider = null)
        => new(repository, unitOfWork, timeProvider ?? TimeProvider.System);

    [Fact]
    public async Task CreateAsync_should_return_created_order()
    {
        var repository = new InMemoryOrderRepository();
        var uow = new MockUnitOfWork();
        var sut = CreateSut(repository, uow, new FixedTimeProvider("2026-04-22T10:00:00Z"));

        var result = await sut.CreateAsync(new CreateOrderRequest(new[] { "XBurger", "Fries", "Soda" }));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(7.60m, result.Value!.Total);
        Assert.Equal(1, repository.Snapshot().Count);
        Assert.Equal(1, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsync_should_return_error_for_invalid_item()
    {
        var repository = new InMemoryOrderRepository();
        var uow = new MockUnitOfWork();
        var sut = CreateSut(repository, uow);

        var result = await sut.CreateAsync(new CreateOrderRequest(new[] { "INVALID" }));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "menu_item_not_found");
        Assert.Empty(repository.Snapshot());
        Assert.Equal(0, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsync_should_return_error_for_duplicate_category()
    {
        var repository = new InMemoryOrderRepository();
        var uow = new MockUnitOfWork();
        var sut = CreateSut(repository, uow);

        var result = await sut.CreateAsync(new CreateOrderRequest(new[] { "XBurger", "XEgg" }));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "duplicated_item");
        Assert.Empty(repository.Snapshot());
        Assert.Equal(0, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task GetByIdAsync_should_return_not_found_when_missing()
    {
        var repository = new InMemoryOrderRepository();
        var uow = new MockUnitOfWork();
        var sut = CreateSut(repository, uow);

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "order_not_found");
    }

    [Fact]
    public async Task UpdateAsync_should_update_existing_order()
    {
        var repository = new InMemoryOrderRepository();
        var uow = new MockUnitOfWork();
        var timeProvider = new FixedTimeProvider("2026-04-22T10:00:00Z");
        var sut = CreateSut(repository, uow, timeProvider);

        var createResult = await sut.CreateAsync(new CreateOrderRequest(new[] { "XBurger" }));
        var id = createResult.Value!.Id;

        timeProvider.SetUtcNow("2026-04-22T10:15:00Z");

        var updateResult = await sut.UpdateAsync(id, new UpdateOrderRequest(new[] { "XBurger", "Soda" }));

        Assert.True(updateResult.IsSuccess);
        Assert.Equal(6.37m, updateResult.Value!.Total);
        Assert.Equal(2, uow.SaveChangesCalls);
    }

    [Fact]
    public async Task DeleteAsync_should_remove_existing_order()
    {
        var repository = new InMemoryOrderRepository();
        var uow = new MockUnitOfWork();
        var sut = CreateSut(repository, uow, new FixedTimeProvider("2026-04-22T10:00:00Z"));

        var create = await sut.CreateAsync(new CreateOrderRequest(new[] { "XBurger" }));
        var id = create.Value!.Id;

        var delete = await sut.DeleteAsync(id);

        Assert.True(delete.IsSuccess);
        Assert.Empty(repository.Snapshot());
        Assert.Equal(2, uow.SaveChangesCalls);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public FixedTimeProvider(string utcNow)
        {
            _utcNow = DateTimeOffset.Parse(utcNow);
        }

        public void SetUtcNow(string utcNow) => _utcNow = DateTimeOffset.Parse(utcNow);

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}