using GoodHamburger.Api.Http;
using GoodHamburger.Application.Contracts;
using GoodHamburger.Application.Services;
using GoodHamburger.Application.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapGet("/", GetAllAsync)
            .WithName("GetOrders")
            .WithSummary("Lista todos os pedidos.")
            .Produces<IReadOnlyList<OrderDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetOrderById")
            .WithSummary("Consulta um pedido por identificador.")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateOrder")
            .WithSummary("Cria um novo pedido.")
            .Produces<OrderDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateOrder")
            .WithSummary("Atualiza um pedido existente.")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteOrder")
            .WithSummary("Remove um pedido.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<OrderDto>>> GetAllAsync(
        [FromQuery] string? searchTerm,
        IOrderService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllAsync(searchTerm, cancellationToken);
        return TypedResults.Ok(result.Value!);
    }

    private static async Task<Results<Ok<OrderDto>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> GetByIdAsync(
        Guid id,
        IOrderService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value!);
        }

        return MapOrderLookupError(result.Errors, $"/api/orders/{id}");
    }

    private static async Task<Results<Created<OrderDto>, BadRequest<ProblemDetails>>> CreateAsync(
        CreateOrderRequest request,
        IOrderService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.Created($"/api/orders/{result.Value!.Id}", result.Value!);
        }

        return MapCreateError(result.Errors, "/api/orders");
    }

    private static async Task<Results<Ok<OrderDto>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> UpdateAsync(
        Guid id,
        UpdateOrderRequest request,
        IOrderService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value!);
        }

        return MapOrderLookupError(result.Errors, $"/api/orders/{id}");
    }

    private static async Task<Results<NoContent, BadRequest<ProblemDetails>, NotFound<ProblemDetails>>> DeleteAsync(
        Guid id,
        IOrderService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        return MapDeleteError(result.Errors, $"/api/orders/{id}");
    }

    private static Results<Ok<OrderDto>, BadRequest<ProblemDetails>, NotFound<ProblemDetails>> MapOrderLookupError(
        IReadOnlyList<ApplicationError> errors,
        string instance)
    {
        var error = errors[0];

        return error.Code switch
        {
            "order_not_found" => ApiProblemDetails.NotFound(error, instance),
            "invalid_order_id" => ApiProblemDetails.BadRequest(error, instance),
            _ => ApiProblemDetails.BadRequest(error, instance)
        };
    }

    private static Results<Created<OrderDto>, BadRequest<ProblemDetails>> MapCreateError(
        IReadOnlyList<ApplicationError> errors,
        string instance)
    {
        var error = errors[0];
        return ApiProblemDetails.BadRequest(error, instance);
    }

    private static Results<NoContent, BadRequest<ProblemDetails>, NotFound<ProblemDetails>> MapDeleteError(
        IReadOnlyList<ApplicationError> errors,
        string instance)
    {
        var error = errors[0];

        return error.Code switch
        {
            "order_not_found" => ApiProblemDetails.NotFound(error, instance),
            "invalid_order_id" => ApiProblemDetails.BadRequest(error, instance),
            _ => ApiProblemDetails.BadRequest(error, instance)
        };
    }
}