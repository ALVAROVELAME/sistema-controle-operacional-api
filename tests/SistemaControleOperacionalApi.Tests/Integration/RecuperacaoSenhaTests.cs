using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SistemaControleOperacionalApi.Tests.Infrastructure;

namespace SistemaControleOperacionalApi.Tests.Integration;

public sealed class RecuperacaoSenhaTests : IntegrationTest
{
    [Fact]
    public async Task Recuperacao_altera_senha_e_token_so_pode_ser_usado_uma_vez()
    {
        await LoginAsync();
        var response = await Client.PostAsJsonAsync("/api/auth/recuperar-senha", new { email = "usuario@example.test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = Factory.Emails.Recovery.Should().ContainSingle().Subject.Token;
        await Factory.WithDatabaseAsync(async db => (await db.Usuarios.SingleAsync()).RecuperacaoTokenHash.Should().NotBe(token));
        var payload = new { token, senha = "NovaSenha123!" };
        (await Client.PostAsJsonAsync("/api/auth/redefinir-senha", payload)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.PostAsJsonAsync("/api/auth/redefinir-senha", payload)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PostAsJsonAsync("/api/auth/login", new { email = "usuario@example.test", senha = Password })).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Client.PostAsJsonAsync("/api/auth/login", new { email = "usuario@example.test", senha = payload.senha })).StatusCode.Should().Be(HttpStatusCode.OK);
    }
    [Fact]
    public async Task Email_inexistente_recebe_mesma_resposta_sem_envio()
    {
        await LoginAsync();
        var inexistente = await Client.PostAsJsonAsync("/api/auth/recuperar-senha", new { email = "ausente@example.test" });
        var existente = await Client.PostAsJsonAsync("/api/auth/recuperar-senha", new { email = "usuario@example.test" });
        (await inexistente.Content.ReadAsStringAsync()).Should().Be(await existente.Content.ReadAsStringAsync());
        Factory.Emails.Recovery.Should().ContainSingle();
    }
    [Fact]
    public async Task Token_expirado_ou_substituido_nao_altera_senha()
    {
        await LoginAsync();
        await Client.PostAsJsonAsync("/api/auth/recuperar-senha", new { email = "usuario@example.test" });
        var antigo = Factory.Emails.Recovery.Single().Token;
        await Client.PostAsJsonAsync("/api/auth/recuperar-senha", new { email = "usuario@example.test" });
        var novo = Factory.Emails.Recovery.Last().Token;
        (await Client.PostAsJsonAsync("/api/auth/redefinir-senha", new { token = antigo, senha = "NovaSenha" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await Factory.WithDatabaseAsync(async db => { (await db.Usuarios.SingleAsync()).RecuperacaoExpiraEm = DateTime.UtcNow.AddMinutes(-1); await db.SaveChangesAsync(); });
        (await Client.PostAsJsonAsync("/api/auth/redefinir-senha", new { token = novo, senha = "NovaSenha" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PostAsJsonAsync("/api/auth/login", new { email = "usuario@example.test", senha = Password })).StatusCode.Should().Be(HttpStatusCode.OK);
    }
    [Fact]
    public async Task Requisicoes_excessivas_sao_limitadas()
    {
        for (var i = 0; i < 10; i++)
            (await Client.PostAsJsonAsync("/api/auth/recuperar-senha", new { email = "ausente@example.test" })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.PostAsJsonAsync("/api/auth/recuperar-senha", new { email = "ausente@example.test" })).StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
    [Fact]
    public async Task Dados_invalidos_sao_rejeitados()
    {
        (await Client.PostAsJsonAsync("/api/auth/recuperar-senha", new { email = "invalido" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PostAsJsonAsync("/api/auth/redefinir-senha", new { token = "invalido", senha = "123" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PostAsJsonAsync("/api/auth/redefinir-senha", new { token = "invalido", senha = "      " })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
