using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaControleOperacionalApi.DTOs.Tarefas;
using SistemaControleOperacionalApi.Extensions;
using SistemaControleOperacionalApi.Services;

namespace SistemaControleOperacionalApi.Controllers;

[ApiController]
[Route("api/tarefas")]
[Authorize]
public class TarefasController : ControllerBase
{
    private readonly TarefaService _tarefaService;
    public TarefasController(TarefaService tarefaService) => _tarefaService = tarefaService;

    private long UsuarioId => User.GetUsuarioId();

    [HttpGet]
    public async Task<ActionResult<List<TarefaResponseDTO>>> Listar()
        => Ok(await _tarefaService.ListarAsync(UsuarioId));

    [HttpPost]
    public async Task<ActionResult<TarefaResponseDTO>> Criar([FromBody] CriarTarefaRequestDTO dto)
    {
        var criada = await _tarefaService.CriarAsync(UsuarioId, dto);
        return CreatedAtAction(nameof(Listar), new { id = criada.Id }, criada);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TarefaResponseDTO>> Atualizar(string id, [FromBody] AtualizarTarefaRequestDTO dto)
    {
        var atualizada = await _tarefaService.AtualizarAsync(UsuarioId, id, dto);
        return atualizada is null ? NotFound() : Ok(atualizada);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> MudarStatus(string id, [FromBody] MudarStatusRequestDTO dto)
    {
        var ok = await _tarefaService.MudarStatusAsync(UsuarioId, id, dto.Status);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id}/pomodoros")]
    public async Task<IActionResult> RegistrarPomodoro(string id)
    {
        var ok = await _tarefaService.RegistrarPomodoroAsync(UsuarioId, id);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(string id)
    {
        var ok = await _tarefaService.ExcluirAsync(UsuarioId, id);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete]
    public async Task<IActionResult> Zerar()
    {
        await _tarefaService.ZerarAsync(UsuarioId);
        return NoContent();
    }

    [HttpPost("importar")]
    public async Task<ActionResult<List<TarefaResponseDTO>>> Importar([FromBody] List<CriarTarefaRequestDTO> itens)
        => Ok(await _tarefaService.ImportarAsync(UsuarioId, itens));
}