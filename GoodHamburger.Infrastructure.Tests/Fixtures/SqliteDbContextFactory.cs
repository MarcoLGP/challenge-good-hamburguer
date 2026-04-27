using GoodHamburger.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Tests.Fixtures;

internal sealed class SqliteDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDbContextFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public GoodHamburgerDbContext Create()
    {
        var options = new DbContextOptionsBuilder<GoodHamburgerDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new GoodHamburgerDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}