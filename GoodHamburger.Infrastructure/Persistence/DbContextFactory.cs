using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GoodHamburger.Infrastructure.Persistence;

public sealed class GoodHamburgerDbContextFactory : IDesignTimeDbContextFactory<GoodHamburgerDbContext>
{
    public GoodHamburgerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GoodHamburgerDbContext>();
        
        // For design time, connect to localhost instead of the Docker service name
        var connectionString = "Server=localhost;Port=3306;Database=GoodHamburgerDB;User=goodhamburger;Password=goodhamburger;";
        
        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure();
            });
        
        return new GoodHamburgerDbContext(optionsBuilder.Options);
    }
}
