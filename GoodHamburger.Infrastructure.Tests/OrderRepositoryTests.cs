using GoodHamburger.Application.Abstractions.Persistence;
using GoodHamburger.Domain.Menu;
using GoodHamburger.Domain.Orders;
using GoodHamburger.Infrastructure.Persistence;
using GoodHamburger.Infrastructure.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GoodHamburger.Infrastructure.Tests;

public class OrderRepositoryTests
{
    [Fact]
    public async Task Add_GetAndRemove_should_work_against_real_DbContext_model()
    {
        using var factory = new SqliteDbContextFactory();
        using var context = factory.Create();

        var services = new ServiceCollection();
        services.AddScoped<IOrderRepository>(_ => new OrderRepository(context));
        services.AddScoped<IUnitOfWork>(_ => context);

        using var provider = services.BuildServiceProvider();
        var repository = provider.GetRequiredService<IOrderRepository>();
        var uow = provider.GetRequiredService<IUnitOfWork>();

        var now = DateTimeOffset.Parse("2026-04-22T10:00:00Z");
        var order = Order.Create(Guid.NewGuid(), [MenuItemCode.XBurger, MenuItemCode.Soda], now);

        await repository.AddAsync(order);
        await uow.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(order.Id);
        Assert.NotNull(loaded);
        Assert.Equal(order.Id, loaded!.Id);
        Assert.Equal(6.37m, loaded.Total);

        repository.Remove(loaded);
        await uow.SaveChangesAsync();

        var afterDelete = await repository.GetByIdAsync(order.Id);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task GetAllAsync_with_search_should_filter_correctly()
    {
        using var factory = new SqliteDbContextFactory();
        using var context = factory.Create();
        var repository = new OrderRepository(context);

        var now = DateTimeOffset.UtcNow;
        var order1 = Order.Create(Guid.NewGuid(), [MenuItemCode.XBurger], now);
        var order2 = Order.Create(Guid.NewGuid(), [MenuItemCode.Soda], now);

        await context.Orders.AddRangeAsync(order1, order2);
        await context.SaveChangesAsync();

        // Search by Burger (XBurger)
        var resultBurger = await repository.GetAllAsync("Burger");
        Assert.Single(resultBurger);
        Assert.Equal(order1.Id, resultBurger[0].Id);

        // Search by Soda
        var resultSoda = await repository.GetAllAsync("Soda");
        Assert.Single(resultSoda);
        Assert.Equal(order2.Id, resultSoda[0].Id);

        // Search by ID
        var resultId = await repository.GetAllAsync(order1.Id.ToString());
        Assert.Single(resultId);
        Assert.Equal(order1.Id, resultId[0].Id);

        // Search for non-existent
        var resultNone = await repository.GetAllAsync("Inexistente");
        Assert.Empty(resultNone);
    }
}