using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SistemaControleOperacionalApi.Tests.Infrastructure;

namespace SistemaControleOperacionalApi.Tests.Integration;

public sealed class AuthTests : IntegrationTest
{
    [Fact]
    public async Task Cadastro_confirmacao_login_e_perfil_com_jwt_real()
    {
        var payload = new { nome = "Maria Teste", email = "maria@example.test", senha = Password };
        var cadastro = await Client.PostAsJsonAsync("/api/usuarios", payload);
        cadastro.StatusCode.Should().Be(HttpStatusCode.OK);
        var email = Factory.Emails.Sent.Should().ContainSingle().Subject;
        email.Email.Should().Be(payload.email);
        await Factory.WithDatabaseAsync(async db =>
        {
            (await db.Usuarios.CountAsync()).Should().Be(0);
            var pending = await db.CadastrosPendentes.SingleAsync();
            pending.SenhaHash.Should().NotBe(Password);
            pending.TokenConfirmacao.Should().Be(email.Token);
        });
        (await Client.GetAsync($"/api/auth/confirmar?token={email.Token}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.GetAsync($"/api/auth/confirmar?token={email.Token}")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { payload.email, payload.senha });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("sucesso").GetBoolean().Should().BeTrue();
        Client.DefaultRequestHeaders.Authorization = new("Bearer", json.GetProperty("token").GetString());
        foreach (var route in new[] { "/api/auth/me", "/api/usuarios/me" })
        {
            var me = await Client.GetAsync(route);
            me.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await me.Content.ReadAsStringAsync();
            body.Should().Contain(payload.email).And.NotContain("senha").And.NotContain("tokenConfirmacao");
        }
        await Factory.WithDatabaseAsync(async db =>
        {
            (await db.CadastrosPendentes.CountAsync()).Should().Be(0);
            (await db.Usuarios.SingleAsync()).Ativo.Should().BeTrue();
        });
    }

    [Theory]
    [InlineData(false, "SenhaTeste123!")]
    [InlineData(true, "SenhaIncorreta")]
    public async Task Login_recusa_conta_inativa_ou_senha_incorreta(bool active, string password)
    {
        await LoginAsync(ativo: active);
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { email = "usuario@example.test", senha = password });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("sucesso").GetBoolean().Should().BeFalse();
        body.GetProperty("token").GetString().Should().BeEmpty();
    }

    [Fact]
    public async Task Login_recusa_usuario_inexistente()
    {
        (await Client.PostAsJsonAsync("/api/auth/login", new { email = "ausente@example.test", senha = Password }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/usuarios", "{\"nome\":\"A\",\"email\":\"invalido\",\"senha\":\"123\"}")]
    [InlineData("/api/auth/login", "{\"email\":\"invalido\",\"senha\":\"123\"}")]
    public async Task Dados_invalidos_retornam_validacao(string route, string json)
    {
        var response = await Client.PostAsync(route, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors").EnumerateObject().Should().NotBeEmpty();
        Factory.Emails.Sent.Should().BeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cadastro_duplicado_retorna_conflito(bool confirmed)
    {
        var payload = new { nome = "Maria Teste", email = "maria@example.test", senha = Password };
        (await Client.PostAsJsonAsync("/api/usuarios", payload)).StatusCode.Should().Be(HttpStatusCode.OK);
        if (confirmed)
            (await Client.GetAsync($"/api/auth/confirmar?token={Factory.Emails.Sent.Single().Token}")).EnsureSuccessStatusCode();
        var response = await Client.PostAsJsonAsync("/api/usuarios", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("sucesso").GetBoolean().Should().BeFalse();
        Factory.Emails.Sent.Should().ContainSingle();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Confirmacao_recusa_token_inexistente_ou_expirado(bool expired)
    {
        var token = "inexistente";
        if (expired)
        {
            await Client.PostAsJsonAsync("/api/usuarios", new { nome = "Maria", email = "maria@example.test", senha = Password });
            token = Factory.Emails.Sent.Single().Token;
            await Factory.WithDatabaseAsync(async db =>
            {
                (await db.CadastrosPendentes.SingleAsync()).TokenExpiraEm = DateTime.UtcNow.AddMinutes(-1);
                await db.SaveChangesAsync();
            });
        }
        (await Client.GetAsync($"/api/auth/confirmar?token={token}")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await Factory.WithDatabaseAsync(async db => (await db.Usuarios.CountAsync()).Should().Be(0));
    }

    [Theory]
    [InlineData("SenhaIncorreta", HttpStatusCode.Unauthorized)]
    [InlineData("SenhaTeste123!", HttpStatusCode.OK)]
    public async Task Exclusao_da_conta_exige_senha(string password, HttpStatusCode expected)
    {
        await LoginAsync();
        (await Client.PostAsJsonAsync("/api/usuarios/me/excluir", new { senha = password })).StatusCode.Should().Be(expected);
        await Factory.WithDatabaseAsync(async db => (await db.Usuarios.CountAsync()).Should().Be(expected == HttpStatusCode.OK ? 0 : 1));
    }

    [Theory]
    [InlineData("GET", "/api/auth/me")]
    [InlineData("GET", "/api/usuarios/me")]
    [InlineData("POST", "/api/usuarios/me/excluir")]
    [InlineData("GET", "/api/tarefas")]
    [InlineData("POST", "/api/tarefas")]
    [InlineData("PUT", "/api/tarefas/1")]
    [InlineData("PATCH", "/api/tarefas/1/status")]
    [InlineData("POST", "/api/tarefas/1/pomodoros")]
    [InlineData("DELETE", "/api/tarefas/1")]
    [InlineData("DELETE", "/api/tarefas")]
    [InlineData("POST", "/api/tarefas/importar")]
    [InlineData("POST", "/api/pomodoros/eventos")]
    public async Task Endpoints_protegidos_exigem_token(string method, string route)
    {
        using var request = new HttpRequestMessage(new(method), route);
        (await Client.SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Jwt_invalido_e_rejeitado()
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token-invalido");
        (await Client.GetAsync("/api/auth/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("signature")]
    [InlineData("issuer")]
    [InlineData("audience")]
    public async Task Jwt_recusa_expiracao_assinatura_emissor_ou_audiencia_invalidos(string invalid)
    {
        var key = invalid == "signature" ? new string('z', 64) : ApiFactory.Secret;
        var now = DateTime.UtcNow;
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: invalid == "issuer" ? "outro" : "tests",
            audience: invalid == "audience" ? "outro" : "tests",
            claims: new[] { new System.Security.Claims.Claim("id", "1") },
            notBefore: now.AddHours(-2),
            expires: invalid == "expired" ? now.AddHours(-1) : now.AddHours(1),
            signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256));
        Client.DefaultRequestHeaders.Authorization = new("Bearer", new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token));
        (await Client.GetAsync("/api/tarefas")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
