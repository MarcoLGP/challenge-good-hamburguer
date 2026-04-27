namespace GoodHamburger.Application.Contracts;

/// <summary>
/// DTO para resposta de autenticação com tokens.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    string TokenType = "Bearer");
