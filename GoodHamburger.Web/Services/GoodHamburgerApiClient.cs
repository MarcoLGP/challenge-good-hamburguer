using System.Net.Http.Json;
using System.Text.Json;
using GoodHamburger.Web.Models;

namespace GoodHamburger.Web.Services;

/// <summary>
/// Cliente HTTP tipado para a API Good Hamburger.
/// Registrado via AddHttpClient&lt;GoodHamburgerApiClient&gt; no Program.cs.
/// </summary>
public sealed class GoodHamburgerApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GoodHamburgerApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<MenuItemDto>> GetMenuAsync(CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<List<MenuItemDto>>("/api/menu", _jsonOpts, ct)
               ?? [];
    }

    public async Task<List<OrderDto>> GetOrdersAsync(string? searchTerm = null, CancellationToken ct = default)
    {
        var url = "/api/orders";
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            url += $"?searchTerm={Uri.EscapeDataString(searchTerm)}";
        }

        return await _http.GetFromJsonAsync<List<OrderDto>>(url, _jsonOpts, ct)
               ?? [];
    }

    public async Task<ApiResult<OrderDto>> GetOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"/api/orders/{id}", ct);
        if (response.IsSuccessStatusCode)
        {
            var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOpts, ct);
            return order is not null
                ? ApiResult<OrderDto>.Ok(order)
                : ApiResult<OrderDto>.Fail("Pedido não encontrado.");
        }
        var err = await ReadProblemAsync(response, ct);
        return ApiResult<OrderDto>.Fail(err);
    }

    public async Task<ApiResult<OrderDto>> CreateOrderAsync(List<string> itemCodes, CancellationToken ct = default)
    {
        var body = new CreateOrderRequest(itemCodes);
        var response = await _http.PostAsJsonAsync("/api/orders", body, _jsonOpts, ct);

        if (response.IsSuccessStatusCode)
        {
            var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOpts, ct);
            return order is not null
                ? ApiResult<OrderDto>.Ok(order)
                : ApiResult<OrderDto>.Fail("Resposta inesperada da API.");
        }
        return ApiResult<OrderDto>.Fail(await ReadProblemAsync(response, ct));
    }

    public async Task<ApiResult<OrderDto>> UpdateOrderAsync(Guid id, List<string> itemCodes, CancellationToken ct = default)
    {
        var body = new UpdateOrderRequest(itemCodes);
        var response = await _http.PutAsJsonAsync($"/api/orders/{id}", body, _jsonOpts, ct);

        if (response.IsSuccessStatusCode)
        {
            var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOpts, ct);
            return order is not null
                ? ApiResult<OrderDto>.Ok(order)
                : ApiResult<OrderDto>.Fail("Resposta inesperada da API.");
        }
        return ApiResult<OrderDto>.Fail(await ReadProblemAsync(response, ct));
    }

    public async Task<ApiResult<bool>> DeleteOrderAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"/api/orders/{id}", ct);
        if (response.IsSuccessStatusCode) return ApiResult<bool>.Ok(true);
        return ApiResult<bool>.Fail(await ReadProblemAsync(response, ct));
    }

    private static async Task<string> ReadProblemAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>(_jsonOpts, ct);
            return problem?.Detail ?? problem?.Title ?? $"Erro HTTP {(int)response.StatusCode}.";
        }
        catch
        {
            return $"Erro HTTP {(int)response.StatusCode}.";
        }
    }
}
