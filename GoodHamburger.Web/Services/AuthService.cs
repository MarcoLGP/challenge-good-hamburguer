using System.Net.Http.Json;
using System.Text.Json;
using GoodHamburger.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace GoodHamburger.Web.Services;

/// <summary>
/// Serviço de autenticação que gerencia tokens usando cookies.
/// Funciona tanto em pré-renderização (HttpContext.Response) quanto em modo interativo (JS Interop).
/// </summary>
public sealed class AuthService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJSRuntime? _jsRuntime;
    private readonly ToastService _toast;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private const string AccessTokenCookie = "gh_access_token";
    private const string RefreshTokenCookie = "gh_refresh_token";
    private const string ExpiresAtCookie = "gh_expires_at";

    public event Action? OnAuthStateChanged;

    public string? GetUserEmail()
    {
        var token = GetAccessToken();
        if (string.IsNullOrEmpty(token)) return null;

        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1];
            
            // Base64Url to Base64
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            using var jsonDoc = JsonDocument.Parse(jsonBytes);
            
            // Tenta 'email' ou 'sub' ou 'unique_name'
            if (jsonDoc.RootElement.TryGetProperty("email", out var emailProp)) return emailProp.GetString();
            if (jsonDoc.RootElement.TryGetProperty("sub", out var subProp)) return subProp.GetString();
            if (jsonDoc.RootElement.TryGetProperty("unique_name", out var nameProp)) return nameProp.GetString();
        }
        catch
        {
            // Ignora erros de parsing
        }

        return null;
    }

    public AuthService(
        IHttpClientFactory httpFactory,
        IHttpContextAccessor httpContextAccessor,
        IJSRuntime? jsRuntime,
        ToastService toast)
    {
        _http = httpFactory.CreateClient("AuthClient");
        _httpContextAccessor = httpContextAccessor;
        _jsRuntime = jsRuntime;
        _toast = toast;
    }

    private string? _accessToken;

    public string? GetAccessToken()
    {
        if (!string.IsNullOrEmpty(_accessToken)) return _accessToken;
        _accessToken = GetCookieValue(AccessTokenCookie);
        return _accessToken;
    }

    public bool IsAuthenticated()
    {
        var token = GetAccessToken();
        if (string.IsNullOrEmpty(token)) return false;

        var expiresAtStr = GetCookieValue(ExpiresAtCookie);
        if (DateTimeOffset.TryParse(expiresAtStr, out var expiresAt))
        {
            return DateTimeOffset.UtcNow.AddSeconds(30) < expiresAt;
        }
        return false;
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, password), _jsonOpts);
        return await HandleAuthResponseAsync(response, "Conta criada com sucesso!");
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password), _jsonOpts);
        return await HandleAuthResponseAsync(response, "Bem-vindo de volta!");
    }

    public async Task<bool> RefreshAsync()
    {
        var accessToken = GetAccessToken();
        var refreshToken = GetCookieValue(RefreshTokenCookie);

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            return false;

        var response = await _http.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(accessToken, refreshToken), _jsonOpts);

        if (response.IsSuccessStatusCode)
        {
            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOpts);
            if (auth is not null)
            {
                StoreTokens(auth);
                return true;
            }
        }

        // Refresh falhou, limpa tudo
        ClearTokens();
        return false;
    }

    public void Logout()
    {
        var refreshToken = GetCookieValue(RefreshTokenCookie);
        if (!string.IsNullOrEmpty(refreshToken))
        {
            // Fire-and-forget logout na API
            _ = _http.PostAsJsonAsync("/api/auth/logout", new LogoutRequest(refreshToken), _jsonOpts);
        }

        ClearTokens();
        _toast.Success("Você saiu da conta.");
    }

    private async Task<bool> HandleAuthResponseAsync(HttpResponseMessage response, string successMessage)
    {
        if (response.IsSuccessStatusCode)
        {
            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOpts);
            if (auth is not null)
            {
                try
                {
                    var finalizeResponse = await _http.PostAsJsonAsync(
                        "/api/auth/finalize-session",
                        new { auth.AccessToken, auth.RefreshToken, auth.AccessTokenExpiresAt },
                        _jsonOpts);

                    if (finalizeResponse.IsSuccessStatusCode)
                    {
                        // ✅ ARMAZENA TOKENS LOCALMENTE
                        StoreTokens(auth);
                        
                        // Mostra feedback
                        _toast.Success(successMessage);
                        
                        return true;
                    }
                    else
                    {
                        _toast.Error("Erro ao finalizar autenticação. Tente novamente.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _toast.Error($"Erro na autenticação: {ex.Message}");
                    return false;
                }
            }
        }
        else
        {
            var error = await ReadErrorAsync(response);
            _toast.Error(error ?? "Erro na autenticação. Verifique suas credenciais.");
        }
        return false;
    }

    private void StoreTokens(AuthResponse auth)
    {
        _accessToken = auth.AccessToken;
        SetCookie(AccessTokenCookie, auth.AccessToken, auth.AccessTokenExpiresAt);
        SetCookie(RefreshTokenCookie, auth.RefreshToken, auth.AccessTokenExpiresAt.AddDays(7));
        SetCookie(ExpiresAtCookie, auth.AccessTokenExpiresAt.ToString("O"), auth.AccessTokenExpiresAt.AddDays(7));
        OnAuthStateChanged?.Invoke();
    }

    private void ClearTokens()
    {
        _accessToken = null;
        DeleteCookie(AccessTokenCookie);
        DeleteCookie(RefreshTokenCookie);
        DeleteCookie(ExpiresAtCookie);
        OnAuthStateChanged?.Invoke();
    }

    private string? GetCookieValue(string key)
    {
        // 1) Tenta ler do HttpContext (pré-renderização ou cookies da requisição inicial)
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx?.Request.Cookies.TryGetValue(key, out var value) == true)
            return value;

        return null;
    }

    private void SetCookie(string key, string value, DateTimeOffset expires)
    {
        var ctx = _httpContextAccessor.HttpContext;

        // Se ainda estamos na fase SSR e a resposta não iniciou, usa HttpContext
        if (ctx is not null && !ctx.Response.HasStarted)
        {
            ctx.Response.Cookies.Append(key, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires
            });
            return;
        }

        // Fallback para JS Interop (modo interativo via SignalR)
        if (_jsRuntime is not null)
        {
            _jsRuntime.InvokeVoidAsync("GoodHamburger.setCookie", key, value, expires.ToString("O"));
        }
    }

    private void DeleteCookie(string key)
    {
        var ctx = _httpContextAccessor.HttpContext;

        // Se ainda estamos na fase SSR e a resposta não iniciou, usa HttpContext
        if (ctx is not null && !ctx.Response.HasStarted)
        {
            ctx.Response.Cookies.Delete(key, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
            return;
        }

        // Fallback para JS Interop (modo interativo via SignalR)
        if (_jsRuntime is not null)
        {
            _jsRuntime.InvokeVoidAsync("GoodHamburger.deleteCookie", key);
        }
    }

    private static async Task<string?> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(_jsonOpts);
            return problem?.Detail ?? problem?.Title;
        }
        catch
        {
            return $"Erro HTTP {(int)response.StatusCode}";
        }
    }
}
