using GoodHamburger.Application.Contracts;
using GoodHamburger.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GoodHamburger.Api.Endpoints;

public static class MenuEndpoints
{
    public static IEndpointRouteBuilder MapMenuEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/menu")
            .WithTags("Menu");

        group.MapGet("/", GetMenu)
            .WithName("GetMenu")
            .WithSummary("Retorna o cardápio disponível.")
            .Produces<IReadOnlyList<MenuItemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static Ok<IReadOnlyList<MenuItemDto>> GetMenu(IMenuService service)
        => TypedResults.Ok(service.GetMenu());
}