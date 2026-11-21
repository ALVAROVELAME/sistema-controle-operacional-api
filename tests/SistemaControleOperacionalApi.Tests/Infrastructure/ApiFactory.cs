using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SistemaControleOperacionalApi.Data;
using SistemaControleOperacionalApi.Services;

namespace SistemaControleOperacionalApi.Tests.Infrastructure;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    public CapturingEmailService Emails { get; } = new();
    public const string Secret = "test-only-secret-with-at-least-32-characters-123456789";

    public ApiFactory() => _connection.Open();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Configuração de host chega antes da leitura no top-level Program.
        builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DB_CONNECTION_STRING"] = "Server=unused;Database=tests;User=unused;Password=unused",
            ["JWT_SECRET"] = Secret,
            ["JWT_ISSUER"] = "tests",
            ["JWT_AUDIENCE"] = "tests",
            ["JWT_EXPIRATION"] = "3600000",
            ["Database:ApplyMigrations"] = "false"
        }));
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddScoped<AppDbContext>(_ => new SqliteTestDbContext(
                new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options));
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(Emails);
        });
    }

    public async Task WithDatabaseAsync(Func<AppDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}

// SQLite nÃ£o ordena DateTimeOffset nativamente. MantÃ©m a ordem temporal em UTC.
public sealed class SqliteTestDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);
        builder.Properties<DateTimeOffset>().HaveConversion<UtcTicksConverter>();
    }
}

public sealed class UtcTicksConverter() : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset, long>(
    value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));

public sealed record SentEmail(string Email, string Nome, string Token);

public sealed class CapturingEmailService : IEmailService
{
    public ConcurrentQueue<SentEmail> Sent { get; } = new();
    public ConcurrentQueue<SentEmail> Recovery { get; } = new();
    public Task EnviarEmailRecuperacaoAsync(string email, string nome, string token) { Recovery.Enqueue(new(email, nome, token)); return Task.CompletedTask; }
    public Task EnviarEmailConfirmacaoAsync(string email, string nome, string token)
    {
        Sent.Enqueue(new(email, nome, token));
        return Task.CompletedTask;
    }
}
