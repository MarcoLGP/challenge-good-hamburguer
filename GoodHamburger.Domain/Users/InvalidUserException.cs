namespace GoodHamburger.Domain.Users;

/// <summary>
/// Exceção lançada quando uma validação de usuário falha.
/// </summary>
public sealed class InvalidUserException : Common.DomainException
{
    public InvalidUserException(string message)
        : base("invalid_user", message) { }
}
