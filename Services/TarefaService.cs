using SistemaControleOperacionalApi.DTOs.Tarefas;
using SistemaControleOperacionalApi.Enums;
using SistemaControleOperacionalApi.Models;
using SistemaControleOperacionalApi.Repositories;

namespace SistemaControleOperacionalApi.Services;

public sealed class TarefaService
{
    private readonly TarefaRepository _repo;
    public TarefaService(TarefaRepository repo) => _repo = repo;

    public async Task<List<TarefaResponseDTO>> ListarAsync(long usuarioId)
    {
        var lista = await _repo.ListarPorUsuarioAsync(usuarioId);
        return lista.Select(ToDto).ToList();
    }

    public async Task<TarefaResponseDTO> CriarAsync(long usuarioId, CriarTarefaRequestDTO dto)
    {
        var t = new Tarefa
        {
            UsuarioId = usuarioId,
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            Prioridade = dto.Prioridade,
            Status = dto.Status,
            Prazo = dto.Prazo,
            PomodorosPlanejados = dto.PomodorosPlanejados,
            Pomodoros = 0,
            CriadoEm = DateTimeOffset.UtcNow,
        };
        await _repo.AddAsync(t);
        return ToDto(t);
    }

    public async Task<TarefaResponseDTO?> AtualizarAsync(long usuarioId, string idRaw, AtualizarTarefaRequestDTO dto)
    {
        if (!long.TryParse(idRaw, out var id)) return null;
        var t = await _repo.FindByIdAsync(id, usuarioId);
        if (t is null) return null;

        t.Titulo = dto.Titulo;
        t.Descricao = dto.Descricao;
        t.Prioridade = dto.Prioridade;
        t.Status = dto.Status;
        t.Prazo = dto.Prazo;
        t.PomodorosPlanejados = dto.PomodorosPlanejados;

        await _repo.UpdateAsync(t);
        return ToDto(t);
    }

    public async Task<bool> MudarStatusAsync(long usuarioId, string idRaw, StatusTarefa status)
    {
        if (!long.TryParse(idRaw, out var id)) return false;
        var t = await _repo.FindByIdAsync(id, usuarioId);
        if (t is null) return false;
        t.Status = status;
        await _repo.UpdateAsync(t);
        return true;
    }

    public async Task<bool> RegistrarPomodoroAsync(long usuarioId, string idRaw)
    {
        if (!long.TryParse(idRaw, out var id)) return false;
        var t = await _repo.FindByIdAsync(id, usuarioId);
        if (t is null) return false;
        t.Pomodoros += 1;
        await _repo.UpdateAsync(t);
        return true;
    }

    public async Task<bool> ExcluirAsync(long usuarioId, string idRaw)
    {
        if (!long.TryParse(idRaw, out var id)) return false;
        var t = await _repo.FindByIdAsync(id, usuarioId);
        if (t is null) return false;
        await _repo.DeleteAsync(t);
        return true;
    }

    public Task ZerarAsync(long usuarioId) => _repo.ZerarAsync(usuarioId);

    public async Task<List<TarefaResponseDTO>> ImportarAsync(long usuarioId, List<CriarTarefaRequestDTO> itens)
    {
        var resultado = new List<TarefaResponseDTO>();
        foreach (var dto in itens)
            resultado.Add(await CriarAsync(usuarioId, dto));
        return resultado;
    }

    private static TarefaResponseDTO ToDto(Tarefa t) => new()
    {
        Id = t.Id.ToString(),
        Titulo = t.Titulo,
        Descricao = t.Descricao,
        Prioridade = t.Prioridade,
        Status = t.Status,
        CriadoEm = t.CriadoEm,
        Prazo = t.Prazo,
        Pomodoros = t.Pomodoros,
        PomodorosPlanejados = t.PomodorosPlanejados,
    };
}