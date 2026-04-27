using GoodHamburger.Api.Http;
using GoodHamburger.Application.Contracts;
using GoodHamburger.Application.Services;
using GoodHamburger.Application.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Registra um novo usuário e retorna tokens JWT.")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Autentica um usuário e retorna tokens JWT.")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/refresh", RefreshAsync)
            .WithName("RefreshToken")
            .WithSummary("Renova o access token usando um refresh token.")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("Revoga o refresh token do usuário.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/finalize-session", FinalizeSessionAsync)
            .WithName("FinalizeSession")
            .WithSummary("Define cookies HttpOnly com os tokens de autenticação.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<Results<Created<AuthResponse>, BadRequest<ProblemDetails>>> RegisterAsync(
        RegisterRequest request,
        IAuthenticationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.RegisterAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.Created("/api/auth/login", result.Value!);
        }

        return MapAuthError(result.Errors, "/api/auth/register");
    }

    private static async Task<Results<Ok<AuthResponse>, BadRequest<ProblemDetails>>> LoginAsync(
        LoginRequest request,
        IAuthenticationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.LoginAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value!);
        }

        return MapAuthError(result.Errors, "/api/auth/login");
    }

    private static async Task<Results<Ok<AuthResponse>, BadRequest<ProblemDetails>>> RefreshAsync(
        RefreshTokenRequest request,
        IAuthenticationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.RefreshAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value!);
        }

        return MapAuthError(result.Errors, "/api/auth/refresh");
    }

    private static async Task<Results<NoContent, BadRequest<ProblemDetails>>> LogoutAsync(
        [FromBody] LogoutRequest request,
        IAuthenticationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.LogoutAsync(request.RefreshToken, cancellationToken);

        if (result.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        return MapAuthError(result.Errors, "/api/auth/logout");
    }

    private static Results<NoContent, BadRequest<ProblemDetails>> FinalizeSessionAsync(
        [FromBody] FinalizeSessionRequest request,
        HttpContext context)
    {
        // Define os cookies HttpOnly de forma segura no servidor
        context.Response.Cookies.Append("gh_access_token", request.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = request.AccessTokenExpiresAt
        });

        context.Response.Cookies.Append("gh_refresh_token", request.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = request.AccessTokenExpiresAt.AddDays(7)
        });

        context.Response.Cookies.Append("gh_expires_at", request.AccessTokenExpiresAt.ToString("O"), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = request.AccessTokenExpiresAt.AddDays(7)
        });

        return TypedResults.NoContent();
    }

    private static BadRequest<ProblemDetails> MapAuthError(
        IReadOnlyList<ApplicationError> errors,
        string instance)
    {
        var error = errors[0];
        return ApiProblemDetails.BadRequest(error, instance);
    }
}

public sealed record LogoutRequest(string RefreshToken);
