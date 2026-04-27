using System.Net;
using System.Net.Http.Json;
using GoodHamburger.Application.Contracts;
using GoodHamburger.Api.Tests.Fixtures;
using Microsoft.AspNetCore.Http;

namespace GoodHamburger.Api.Tests;

/// <summary>
/// Testes de integração para endpoints de autenticação.
/// Valida o fluxo completo: login → finalize-session → cookies.
/// </summary>
public sealed class AuthEndpointsTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private HttpClient _client = null!;

    public AuthEndpointsTests()
    {
        _factory = new ApiFactory();
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        // Aguarda a API estar pronta
        await Task.Delay(500);
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact(DisplayName = "Register deve retornar tokens válidos")]
    public async Task Register_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest("newuser@test.com", "password123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.NotEmpty(auth.AccessToken);
        Assert.NotEmpty(auth.RefreshToken);
        Assert.True(auth.AccessTokenExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact(DisplayName = "Login com credenciais válidas deve retornar tokens")]
    public async Task Login_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange - primeiro registra um usuário
        var registerReq = new RegisterRequest("logintest@test.com", "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerReq);

        // Act - faz login com mesmas credenciais
        var loginReq = new LoginRequest("logintest@test.com", "password123");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginReq);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.NotEmpty(auth.AccessToken);
        Assert.NotEmpty(auth.RefreshToken);
    }

    [Fact(DisplayName = "Login com credenciais inválidas deve falhar")]
    public async Task Login_WithInvalidCredentials_ShouldFail()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@test.com", "wrongpassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "FinalizeSession deve definir cookies HttpOnly")]
    public async Task FinalizeSession_ShouldSetHttpOnlyCookies()
    {
        // Arrange - registra e obtém tokens
        var registerReq = new RegisterRequest("finalizetest@test.com", "password123");
        var registerResp = await _client.PostAsJsonAsync("/api/auth/register", registerReq);
        var auth = await registerResp.Content.ReadFromJsonAsync<AuthResponse>();
        
        Assert.NotNull(auth);

        // Act - chama o endpoint de finalizar sessão
        var finalizeReq = new { auth.AccessToken, auth.RefreshToken, auth.AccessTokenExpiresAt };
        var finalizeResp = await _client.PostAsJsonAsync("/api/auth/finalize-session", finalizeReq);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, finalizeResp.StatusCode);
        
        // Verifica se os cookies foram definidos
        Assert.NotEmpty(finalizeResp.Headers.GetValues("Set-Cookie"));
        
        var setCookies = finalizeResp.Headers.GetValues("Set-Cookie").ToList();
        var hasAccessToken = setCookies.Any(c => c.StartsWith("gh_access_token=") && c.Contains("HttpOnly"));
        var hasRefreshToken = setCookies.Any(c => c.StartsWith("gh_refresh_token=") && c.Contains("HttpOnly"));
        var hasExpiresAt = setCookies.Any(c => c.StartsWith("gh_expires_at=") && c.Contains("HttpOnly"));

        Assert.True(hasAccessToken, "Cookie gh_access_token não foi definido ou não é HttpOnly");
        Assert.True(hasRefreshToken, "Cookie gh_refresh_token não foi definido ou não é HttpOnly");
        Assert.True(hasExpiresAt, "Cookie gh_expires_at não foi definido ou não é HttpOnly");
    }

    [Fact(DisplayName = "Todos os cookies devem ser Secure e SameSite=Strict")]
    public async Task FinalizeSession_CookiesShouldBeSecure()
    {
        // Arrange
        var registerReq = new RegisterRequest("securetest@test.com", "password123");
        var registerResp = await _client.PostAsJsonAsync("/api/auth/register", registerReq);
        var auth = await registerResp.Content.ReadFromJsonAsync<AuthResponse>();

        // Act
        var finalizeReq = new { auth.AccessToken, auth.RefreshToken, auth.AccessTokenExpiresAt };
        var finalizeResp = await _client.PostAsJsonAsync("/api/auth/finalize-session", finalizeReq);

        // Assert
        var setCookies = finalizeResp.Headers.GetValues("Set-Cookie");
        
        foreach (var cookie in setCookies)
        {
            Assert.Contains("HttpOnly", cookie, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Secure", cookie, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("SameSite=Strict", cookie);
        }
    }

    [Fact(DisplayName = "Refresh com tokens válidos deve retornar novos tokens")]
    public async Task Refresh_WithValidTokens_ShouldReturnNewAuthResponse()
    {
        // Arrange
        var registerReq = new RegisterRequest("refreshtest@test.com", "password123");
        var registerResp = await _client.PostAsJsonAsync("/api/auth/register", registerReq);
        var initialAuth = await registerResp.Content.ReadFromJsonAsync<AuthResponse>();

        // Act
        var refreshReq = new RefreshTokenRequest(initialAuth.AccessToken, initialAuth.RefreshToken);
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshReq);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var newAuth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(newAuth);
        Assert.NotEmpty(newAuth.AccessToken);
        Assert.NotEmpty(newAuth.RefreshToken);
    }

    [Fact(DisplayName = "Logout deve revogar o token")]
    public async Task Logout_ShouldRevokeToken()
    {
        // Arrange
        var registerReq = new RegisterRequest("logouttest@test.com", "password123");
        var registerResp = await _client.PostAsJsonAsync("/api/auth/register", registerReq);
        var auth = await registerResp.Content.ReadFromJsonAsync<AuthResponse>();

        // Act
        var logoutReq = new { RefreshToken = auth.RefreshToken };
        var response = await _client.PostAsJsonAsync("/api/auth/logout", logoutReq);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Tenta refresh com o token revogado - deve falhar
        var refreshReq = new RefreshTokenRequest(auth.AccessToken, auth.RefreshToken);
        var refreshResp = await _client.PostAsJsonAsync("/api/auth/refresh", refreshReq);
        Assert.Equal(HttpStatusCode.BadRequest, refreshResp.StatusCode);
    }
}
