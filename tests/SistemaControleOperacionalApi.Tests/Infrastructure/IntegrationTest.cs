using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SistemaControleOperacionalApi.DTOs.Auth;
using SistemaControleOperacionalApi.Models;
using SistemaControleOperacionalApi.Services;

namespace SistemaControleOperacionalApi.Tests.Infrastructure;

public abstract class IntegrationTest : IAsyncLifetime
{
    protected ApiFactory Factory { get; } = new();
    protected HttpClient Client { get; private set; } = null!;
    protected const string Password = "SenhaTeste123!";
    private static readonly string PasswordHash = new PasswordHasher().Hash(Password);

    public async Task InitializeAsync()
    {
        Client = Factory.CreateClient(new() { AllowAutoRedirect = false });
        await Factory.WithDatabaseAsync(db => db.Database.EnsureCreatedAsync());
    }

    protected async Task<long> LoginAsync(string email = "usuario@example.test", bool ativo = true)
    {
        var user = new Usuario { Nome = "Usuario Teste", Email = email, SenhaHash = PasswordHash, Ativo = ativo };
        await Factory.WithDatabaseAsync(async db => { db.Usuarios.Add(user); await db.SaveChangesAsync(); });
        if (ativo)
        {
            using var response = await Client.PostAsJsonAsync("/api/auth/login", new { email, senha = Password });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var login = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        }
        return user.Id;
    }

    protected async Task<JsonElement> CreateTaskAsync(string title = "Tarefa de teste")
    {
        using var response = await Client.PostAsJsonAsync("/api/tarefas", new { titulo = title });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return Task.CompletedTask;
    }
}
