namespace GoodHamburger.Web.Services;

/// <summary>
/// Handler HTTP que injeta o Bearer token em todas as requisições.
/// Se o token estiver próximo de expirar, tenta refresh silencioso.
/// </summary>
public sealed class AuthDelegatingHandler : DelegatingHandler
{
    private readonly AuthService _auth;

    public AuthDelegatingHandler(AuthService auth)
    {
        _auth = auth;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Não adiciona auth em endpoints de auth
        if (request.RequestUri?.AbsolutePath.StartsWith("/api/auth/") == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = _auth.GetAccessToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Se recebeu 401, tenta refresh e reenvia uma vez
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            response.Dispose();

            var refreshed = await _auth.RefreshAsync();
            if (refreshed)
            {
                token = _auth.GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                return await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }
}
