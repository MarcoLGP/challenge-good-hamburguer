namespace GoodHamburger.Application.Contracts;

/// <summary>
/// DTO para requisição de renovação de token.
/// </summary>
public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);
