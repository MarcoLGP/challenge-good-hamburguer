using GoodHamburger.Api.Tests.Mocks;
using GoodHamburger.Application.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GoodHamburger.Api.Tests.Fixtures;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IOrderService>();
            services.RemoveAll<IMenuService>();

            services.AddSingleton<IOrderService, StubOrderService>();
            services.AddSingleton<IMenuService, StubMenuService>();
        });
    }
}