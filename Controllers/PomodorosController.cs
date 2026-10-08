using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaControleOperacionalApi.DTOs.Pomodoros;
using SistemaControleOperacionalApi.Extensions;
using SistemaControleOperacionalApi.Models;
using SistemaControleOperacionalApi.Repositories;

namespace SistemaControleOperacionalApi.Controllers;

[ApiController]
[Route("api/pomodoros")]
[Authorize]
public class PomodorosController : ControllerBase
{
    private readonly PomodoroEventoRepository _repo;
    public PomodorosController(PomodoroEventoRepository repo) => _repo = repo;

    [HttpPost("eventos")]
    public async Task<IActionResult> RegistrarEvento([FromBody] EventoPomodoroRequestDTO dto)
    {
        long? tarefaId = null;
        if (!string.IsNullOrWhiteSpace(dto.TarefaId) && long.TryParse(dto.TarefaId, out var parsed))
            tarefaId = parsed;

        await _repo.AddAsync(new PomodoroEvento
        {
            UsuarioId = User.GetUsuarioId(),
            Tipo = dto.Tipo,
            Modo = dto.Modo,
            Ciclo = dto.Ciclo,
            MinutosPlanejados = dto.MinutosPlanejados,
            MinutosReais = dto.MinutosReais,
            SegundosReais = dto.SegundosReais,
            OcorridoEm = dto.OcorridoEm,
            TarefaId = tarefaId,
            TarefaTitulo = dto.TarefaTitulo,
            StatusTarefa = dto.StatusTarefa,
        });

        return NoContent();
    }
}