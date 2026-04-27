using GoodHamburger.Application.Abstractions.Persistence;
using GoodHamburger.Application.Abstractions.Security;
using GoodHamburger.Infrastructure.Persistence;
using GoodHamburger.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoodHamburger.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoodHamburgerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString,
        Action<MySqlDbContextOptionsBuilder>? mysqlOptions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddDbContext<GoodHamburgerDbContext>(options =>
        {
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mySqlOptions =>
                {
                    mySqlOptions.EnableRetryOnFailure();
                    mysqlOptions?.Invoke(mySqlOptions);
                });
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<GoodHamburgerDbContext>());

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IRefreshTokenHasher, RefreshTokenHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
