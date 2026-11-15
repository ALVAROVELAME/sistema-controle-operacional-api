using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SistemaControleOperacionalApi.Enums;
using SistemaControleOperacionalApi.Tests.Infrastructure;

namespace SistemaControleOperacionalApi.Tests.Integration;

public sealed class PomodorosTests : IntegrationTest
{
    [Fact]
    public async Task Evento_persiste_dados_e_usuario_do_token()
    {
        var user = await LoginAsync();
        var task = await CreateTaskAsync();
        var id = task.GetProperty("id").GetString();
        var occurred = DateTimeOffset.UtcNow;
        var response = await Client.PostAsJsonAsync("/api/pomodoros/eventos", new
        {
            tipo = "foco_completado", modo = "foco", ciclo = 1, minutosPlanejados = 25,
            minutosReais = 25, segundosReais = 1500, ocorridoEm = occurred,
            tarefaId = id, tarefaTitulo = "Tarefa de teste", statusTarefa = "em_progresso", usuarioId = 999
        });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await Factory.WithDatabaseAsync(async db =>
        {
            var item = await db.PomodoroEventos.SingleAsync();
            item.UsuarioId.Should().Be(user);
            item.Tipo.Should().Be(TipoEventoPomodoro.FocoCompletado);
            item.Modo.Should().Be(ModoPomodoro.Foco);
            item.TarefaId.Should().Be(long.Parse(id!));
            item.SegundosReais.Should().Be(1500);
            item.OcorridoEm.Should().Be(occurred);
        });
    }

    [Theory]
    [InlineData(0, 25, 1500)]
    [InlineData(9, 25, 1500)]
    [InlineData(1, 121, 1500)]
    [InlineData(1, 25, 7201)]
    [InlineData(1, -1, -1)]
    public async Task Evento_invalido_nao_e_persistido(int cycle, int minutes, int seconds)
    {
        await LoginAsync();
        var response = await Client.PostAsJsonAsync("/api/pomodoros/eventos", new { tipo = "foco_completado", modo = "foco", ciclo = cycle, minutosReais = minutes, segundosReais = seconds, ocorridoEm = DateTimeOffset.UtcNow });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await Factory.WithDatabaseAsync(async db => (await db.PomodoroEventos.CountAsync()).Should().Be(0));
    }
}
