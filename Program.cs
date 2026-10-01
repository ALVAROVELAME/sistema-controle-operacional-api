using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SistemaControleOperacionalApi.Config;
using SistemaControleOperacionalApi.Data;
using SistemaControleOperacionalApi.Endpoints;
using SistemaControleOperacionalApi.Exceptions;
using SistemaControleOperacionalApi.Extensions;
using SistemaControleOperacionalApi.Models;
using SistemaControleOperacionalApi.Repositories;
using SistemaControleOperacionalApi.Security;
using SistemaControleOperacionalApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. CONFIGURACAO TIPADA
// ============================================================
var jwtSettings = JwtSettings.FromEnvironment();
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<JwtTokenGenerator>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<EmailService>();

// Repositorios + Services (Scoped = uma instancia por request)
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<CadastroPendenteRepository>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<EmailConfirmationService>();
builder.Services.AddScoped<AuthService>();

// ============================================================
// 2. BANCO DE DADOS
// ============================================================
var connectionString =
    $"Server={Environment.GetEnvironmentVariable("DB_SERVER")};" +
    $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
    $"Database={Environment.GetEnvironmentVariable("DB_DATABASE")};" +
    $"User={Environment.GetEnvironmentVariable("DB_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

// ============================================================
// 3. AUTENTICACAO JWT
// ============================================================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();

// ============================================================
// 4. CORS
// ============================================================
var frontendUrl = Environment.GetEnvironmentVariable("APP_FRONTEND_URL")
    ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ============================================================
// 5. EXCEPTION HANDLER GLOBAL
// ============================================================
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ============================================================
// 6. SWAGGER com Bearer (sempre ativo, igual ao projeto Java)
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sistema Controle Operacional API",
        Version = "v1"
    });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT: Bearer {token}"
    };

    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ============================================================
// 7. PIPELINE
// ============================================================
app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

// Cria banco/tabelas automaticamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ============================================================
// 8. ENDPOINTS
// ============================================================

// Health check - publico
app.MapGet("/", () => Results.Ok(new
{
    status = "API Sistema de Controle Operacional funcionando"
}))
.AllowAnonymous();

// Autenticacao
app.MapAuthEndpoints();
app.MapUsuarioEndpoints();

// ---------- Endpoint publico de exemplo ----------
app.MapPost("/clientes", async (Cliente cliente, AppDbContext db) =>
{
    db.Clientes.Add(cliente);
    await db.SaveChangesAsync();
    return Results.Ok(cliente);
})
.AllowAnonymous();

// ---------- Endpoint protegido de exemplo ----------
app.MapGet("/clientes", async (AppDbContext db) =>
{
    return await db.Clientes.ToListAsync();
})
.RequireAuthorization();

// ============================================================
// 9. START
// ============================================================
var porta = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{porta}");