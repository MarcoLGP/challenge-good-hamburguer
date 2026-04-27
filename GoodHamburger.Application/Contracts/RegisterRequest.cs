namespace GoodHamburger.Application.Contracts;

/// <summary>
/// DTO para requisição de registro de novo usuário.
/// </summary>
public sealed record RegisterRequest(string Email, string Password);
