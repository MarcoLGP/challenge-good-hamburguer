namespace GoodHamburger.Application.Abstractions.Security;

/// <summary>
/// Abstração para hash e verificação de senhas.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Gera o hash de uma senha em texto plano.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifica se uma senha em texto plano corresponde ao hash armazenado.
    /// </summary>
    bool Verify(string password, string hash);
}
