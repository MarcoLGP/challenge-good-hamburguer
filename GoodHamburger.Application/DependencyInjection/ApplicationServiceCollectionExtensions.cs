using GoodHamburger.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GoodHamburger.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddGoodHamburgerApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
