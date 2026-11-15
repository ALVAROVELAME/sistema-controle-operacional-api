using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SistemaControleOperacionalApi.Tests.Infrastructure;

namespace SistemaControleOperacionalApi.Tests.Integration;

public sealed class TarefasTests : IntegrationTest
{
    [Fact]
    public async Task Crud_preserva_contrato_json_e_contador_controlado_pelo_servidor()
    {
        await LoginAsync();
        var create = await Client.PostAsJsonAsync("/api/tarefas", new { titulo = "Estudar", prioridade = "alta", status = "a_fazer", prazo = "2027-01-20", pomodoros = 99, pomodorosPlanejados = 3 });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        create.Headers.Location.Should().NotBeNull();
        var task = await create.Content.ReadFromJsonAsync<JsonElement>();
        task.GetProperty("pomodoros").GetInt32().Should().Be(0);
        task.GetProperty("status").GetString().Should().Be("a_fazer");
        task.GetProperty("prazo").GetString().Should().Be("2027-01-20");
        var id = task.GetProperty("id").GetString();
        (await Client.PostAsync($"/api/tarefas/{id}/pomodoros", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var update = await Client.PutAsJsonAsync($"/api/tarefas/{id}", new { titulo = "Revisar", status = "em_progresso", prioridade = "baixa", pomodoros = 100 });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        (await update.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("pomodoros").GetInt32().Should().Be(1);
        (await Client.PatchAsJsonAsync($"/api/tarefas/{id}/status", new { status = "concluido" })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var list = await Client.GetFromJsonAsync<JsonElement[]>("/api/tarefas");
        list.Should().ContainSingle().Which.GetProperty("status").GetString().Should().Be("concluido");
        (await Client.DeleteAsync($"/api/tarefas/{id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Client.GetFromJsonAsync<JsonElement[]>("/api/tarefas")).Should().BeEmpty();
    }

    [Theory]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    [InlineData("POST")]
    public async Task Usuario_nao_pode_alterar_tarefa_de_outro(string method)
    {
        await LoginAsync();
        var task = await CreateTaskAsync();
        var id = task.GetProperty("id").GetString();
        await LoginAsync("outro@example.test");
        (await Client.GetFromJsonAsync<JsonElement[]>("/api/tarefas")).Should().BeEmpty();
        var route = $"/api/tarefas/{id}" + (method == "PATCH" ? "/status" : method == "POST" ? "/pomodoros" : "");
        using var request = new HttpRequestMessage(new(method), route) { Content = JsonContent.Create(new { titulo = "Invasao", status = "concluido" }) };
        (await Client.SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        await Factory.WithDatabaseAsync(async db =>
        {
            var persisted = await db.Tarefas.SingleAsync();
            persisted.Titulo.Should().Be("Tarefa de teste");
            persisted.Pomodoros.Should().Be(0);
            persisted.Status.Should().Be(SistemaControleOperacionalApi.Enums.StatusTarefa.AFazer);
        });
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("99999")]
    public async Task Id_invalido_ou_inexistente_retorna_404(string id)
    {
        await LoginAsync();
        (await Client.PutAsJsonAsync($"/api/tarefas/{id}", new { titulo = "Teste" })).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Client.PatchAsJsonAsync($"/api/tarefas/{id}/status", new { status = "concluido" })).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Client.PostAsync($"/api/tarefas/{id}/pomodoros", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Client.DeleteAsync($"/api/tarefas/{id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData("Valida", 0)]
    [InlineData("Valida", 21)]
    public async Task Validacao_rejeita_titulo_ou_estimativa_invalidos(string title, int planned)
    {
        await LoginAsync();
        var task = await CreateTaskAsync();
        var payload = new { titulo = title, pomodorosPlanejados = planned };
        (await Client.PostAsJsonAsync("/api/tarefas", payload)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PutAsJsonAsync($"/api/tarefas/{task.GetProperty("id").GetString()}", payload)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await Factory.WithDatabaseAsync(async db => (await db.Tarefas.SingleAsync()).Titulo.Should().Be("Tarefa de teste"));
    }

    [Fact]
    public async Task Importacao_e_limpeza_respeitam_usuario()
    {
        var owner = await LoginAsync();
        await CreateTaskAsync("Preservar");
        await LoginAsync("outro@example.test");
        var response = await Client.PostAsJsonAsync("/api/tarefas/importar", new[] { new { titulo = "Primeira" }, new { titulo = "Segunda" } });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<JsonElement[]>()).Should().HaveCount(2);
        (await Client.GetFromJsonAsync<JsonElement[]>("/api/tarefas")).Should().HaveCount(2);
        (await Client.DeleteAsync("/api/tarefas")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Client.GetFromJsonAsync<JsonElement[]>("/api/tarefas")).Should().BeEmpty();
        await Factory.WithDatabaseAsync(async db => (await db.Tarefas.SingleAsync()).UsuarioId.Should().Be(owner));
    }

    [Fact]
    public async Task Importacao_invalida_nao_persiste_lote_parcial()
    {
        await LoginAsync();
        (await Client.PostAsJsonAsync("/api/tarefas/importar", new[] { new { titulo = "Valida" }, new { titulo = "" } })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.GetFromJsonAsync<JsonElement[]>("/api/tarefas")).Should().BeEmpty();
    }
}
