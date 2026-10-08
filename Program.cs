using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SistemaControleOperacionalApi.Config;
using SistemaControleOperacionalApi.Data;
using SistemaControleOperacionalApi.Exceptions;
using SistemaControleOperacionalApi.Repositories;
using SistemaControleOperacionalApi.Security;
using SistemaControleOperacionalApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Detecta se está rodando via 'dotnet ef' (design-time)
var isDesignTime = AppDomain.CurrentDomain.GetAssemblies()
    .Any(a => a.FullName?.Contains("EntityFrameworkCore.Design") == true);

// ------------------------------------------------------------
// 1. Controllers + JSON
// ------------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ------------------------------------------------------------
// 2. EF Core + MariaDB/MySQL (lê variáveis individuais do .env)
// ------------------------------------------------------------
string? connectionString;

if (isDesignTime)
{
    connectionString = "Server=localhost;Port=3306;Database=design_time;User=root;Password=root;";
}
else
{
    connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        var server = Environment.GetEnvironmentVariable("DB_SERVER")
            ?? throw new InvalidOperationException("DB_SERVER nao configurado no ambiente.");
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
        var database = Environment.GetEnvironmentVariable("DB_DATABASE")
            ?? throw new InvalidOperationException("DB_DATABASE nao configurado no ambiente.");
        var user = Environment.GetEnvironmentVariable("DB_USER")
            ?? throw new InvalidOperationException("DB_USER nao configurado no ambiente.");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD")
            ?? throw new InvalidOperationException("DB_PASSWORD nao configurado no ambiente.");

        connectionString = $"Server={server};Port={port};Database={database};User={user};Password={password};";
    }
}

var finalConnectionString = connectionString!;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (isDesignTime)
        options.UseMySql(finalConnectionString, new MySqlServerVersion(new Version(8, 0, 36)));
    else
        options.UseMySql(finalConnectionString, ServerVersion.AutoDetect(finalConnectionString));
});

// ------------------------------------------------------------
// 3. CORS
// ------------------------------------------------------------
builder.Services.AddCors(o =>
{
    o.AddPolicy("frontend", p => p
        .WithOrigins(
            "https://ctoperacional.vercel.app",
            "http://localhost:5173",
            "http://localhost:4173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// ------------------------------------------------------------
// 4. JWT (lido do .env)
// ------------------------------------------------------------
JwtSettings jwtSettings;
if (isDesignTime)
{
    jwtSettings = new JwtSettings
    {
        Secret = new string('x', 64),
        Expiration = 86_400_000,
        Issuer = "sistema-controle-operacional-api",
        Audience = "sistema-controle-operacional-api",
    };
}
else
{
    jwtSettings = JwtSettings.FromEnvironment();
}

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<JwtTokenGenerator>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

// ------------------------------------------------------------
// 5. Exception handler global
// ------------------------------------------------------------
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ------------------------------------------------------------
// 6. DI — Repositórios e Serviços
// ------------------------------------------------------------
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<CadastroPendenteRepository>();
builder.Services.AddScoped<TarefaRepository>();
builder.Services.AddScoped<PomodoroEventoRepository>();

builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<EmailService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<EmailConfirmationService>();
builder.Services.AddScoped<TarefaService>();

// ------------------------------------------------------------
// 7. Swagger
// ------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CtOperacional API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Ex: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
});

var app = builder.Build();

// ------------------------------------------------------------
// 8. Migrations só rodam em runtime (nunca em design-time)
// ------------------------------------------------------------
if (!isDesignTime)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ------------------------------------------------------------
// 9. Pipeline
// ------------------------------------------------------------
app.UseExceptionHandler();

// Swagger habilitado em Development OU quando SWAGGER_ENABLED=true
var swaggerEnabled = app.Environment.IsDevelopment()
    || Environment.GetEnvironmentVariable("SWAGGER_ENABLED") == "true";

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();