namespace GoodHamburger.Application.Abstractions.Security;

/// <summary>
/// Abstração para hash e verificação de refresh tokens.
/// Segue a prática de segurança de nunca armazenar refresh tokens em texto puro.
/// </summary>
public interface IRefreshTokenHasher
{
    /// <summary>
    /// Gera o hash SHA256 de um refresh token.
    /// </summary>
    string Hash(string refreshToken);

    /// <summary>
    /// Verifica se um refresh token em texto plano corresponde ao hash armazenado.
    /// </summary>
    bool Verify(string refreshToken, string hash);
}
