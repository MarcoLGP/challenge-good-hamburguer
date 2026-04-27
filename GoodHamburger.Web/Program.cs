using GoodHamburger.Web;
using GoodHamburger.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("'ApiBaseUrl' não configurado em appsettings.json.");

// ── HttpContextAccessor para auth em pré-renderização ────────────────────
builder.Services.AddHttpContextAccessor();

// ── HttpClient com Auth Handler ───────────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthDelegatingHandler>();

// HttpClient para auth (SEM handler de auth — evita ciclo de dependência)
builder.Services.AddHttpClient("AuthClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClient para API geral (COM handler de auth)
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthDelegatingHandler>();

builder.Services.AddHttpClient<GoodHamburgerApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthDelegatingHandler>();

// ── Serviços da aplicação Web ─────────────────────────────────────────────
builder.Services.AddSingleton<ToastService>();

// ── Build ──────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
