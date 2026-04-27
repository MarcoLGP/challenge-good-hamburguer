using GoodHamburger.Api.Tests.Fixtures;
using System.Net;
using Xunit;

namespace GoodHamburger.Api.Tests;

public class MenuEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public MenuEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_menu_should_return_200_and_payload()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/menu");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("XBurger", body);
        Assert.Contains("Batata frita", body);
    }
}