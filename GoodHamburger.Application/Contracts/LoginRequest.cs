namespace GoodHamburger.Application.Contracts;

/// <summary>
/// DTO para requisição de login.
/// </summary>
public sealed record LoginRequest(string Email, string Password);
