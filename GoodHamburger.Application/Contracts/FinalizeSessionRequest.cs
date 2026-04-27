namespace GoodHamburger.Application.Contracts;

/// <summary>
/// DTO para requisição de finalização de sessão.
/// Recebe os tokens do cliente e os persiste em cookies HttpOnly no servidor.
/// </summary>
public sealed record FinalizeSessionRequest(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt);
