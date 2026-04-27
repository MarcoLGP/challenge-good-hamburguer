using GoodHamburger.Domain.Users;

namespace GoodHamburger.Application.Abstractions.Security;

/// <summary>
/// Representa os tokens gerados durante a autenticação.
/// </summary>
public sealed record TokenPair(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt);

/// <summary>
/// Abstração para geração e validação de tokens JWT.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Gera um access token JWT para o usuário informado.
    /// </summary>
    string GenerateAccessToken(User user, DateTimeOffset utcNow, out DateTimeOffset expiresAt);

    /// <summary>
    /// Gera um refresh token criptograficamente seguro.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Valida um access token JWT e retorna o ID do usuário.
    /// </summary>
    Guid? ValidateAccessToken(string accessToken);

    /// <summary>
    /// Extrai o ID do usuário de um access token expirado (para refresh).
    /// </summary>
    Guid? ExtractUserIdFromExpiredToken(string accessToken);
}
