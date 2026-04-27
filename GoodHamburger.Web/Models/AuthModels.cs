namespace GoodHamburger.Web.Models;

public sealed record LoginRequest(string Email, string Password);
public sealed record RegisterRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    string TokenType = "Bearer");

public sealed class AuthState
{
    public bool IsAuthenticated { get; set; }
    public string? Email { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}
