using System.Text;
using GoodHamburger.Api.Endpoints;
using GoodHamburger.Api.Handlers;
using GoodHamburger.Api.Serialization;
using GoodHamburger.Application.DependencyInjection;
using GoodHamburger.Infrastructure.DependencyInjection;
using GoodHamburger.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ── JWT Configuration ─────────────────────────────────────────────────────
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException($"Configuração '{JwtOptions.SectionName}' não encontrada.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── CORS Configuration ────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWeb", policy =>
    {
        policy
            .WithOrigins("https://localhost:7161", "http://localhost:5147")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddOpenApi();
builder.Services.AddGoodHamburgerApplication();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

builder.Services.AddGoodHamburgerInfrastructure(builder.Configuration, connectionString);

var app = builder.Build();

// ── Create Database Schema ────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GoodHamburger.Infrastructure.Persistence.GoodHamburgerDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseExceptionHandler();

// ── Enable CORS ───────────────────────────────────────────────────────────
app.UseCors("AllowWeb");

// ── Authentication & Authorization ────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => TypedResults.Redirect("/scalar/v1"));

app.MapMenuEndpoints();
app.MapOrderEndpoints();
app.MapAuthEndpoints();

app.Run();
