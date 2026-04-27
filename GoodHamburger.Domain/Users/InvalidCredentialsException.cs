namespace GoodHamburger.Domain.Users;

/// <summary>
/// Exceção lançada quando as credenciais de autenticação são inválidas.
/// </summary>
public sealed class InvalidCredentialsException : Common.DomainException
{
    public InvalidCredentialsException(string message)
        : base("invalid_credentials", message) { }
}
