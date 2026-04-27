using GoodHamburger.Api.Tests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace GoodHamburger.Api.Tests;

public class OrderEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public OrderEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_order_should_return_201()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            itemCodes = new[] { "XBurger" }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Get_unknown_order_should_return_404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}