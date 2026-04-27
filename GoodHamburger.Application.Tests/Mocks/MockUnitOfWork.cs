using GoodHamburger.Application.Abstractions.Persistence;

namespace GoodHamburger.Application.Tests.Mocks;

internal sealed class MockUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}
