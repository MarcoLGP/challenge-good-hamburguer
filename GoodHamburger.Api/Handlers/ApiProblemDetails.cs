using GoodHamburger.Application.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.Api.Http;

internal static class ApiProblemDetails
{
    public static ProblemDetails Create(ApplicationError error, int statusCode, string title, string? instance = null)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = error.Message,
            Instance = instance
        };

        problem.Extensions["code"] = error.Code;

        return problem;
    }

    public static BadRequest<ProblemDetails> BadRequest(ApplicationError error, string? instance = null)
        => TypedResults.BadRequest(Create(error, StatusCodes.Status400BadRequest, "Requisição inválida", instance));

    public static NotFound<ProblemDetails> NotFound(ApplicationError error, string? instance = null)
        => TypedResults.NotFound(Create(error, StatusCodes.Status404NotFound, "Recurso não encontrado", instance));

    public static UnauthorizedHttpResult Unauthorized(ApplicationError error, string? instance = null)
        => TypedResults.Unauthorized();
}
