using System.ComponentModel.DataAnnotations;
using SistemaControleOperacionalApi.Enums;

namespace SistemaControleOperacionalApi.DTOs.Tarefas;

/// <summary>
/// PUT: NÃO inclui Pomodoros. O contador pertence ao servidor.
/// </summary>
public class AtualizarTarefaRequestDTO
{
    [Required, MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Descricao { get; set; }

    public Prioridade Prioridade { get; set; } = Prioridade.Media;

    public StatusTarefa Status { get; set; } = StatusTarefa.AFazer;

    public DateOnly? Prazo { get; set; }

    [Range(1, 20)]
    public int? PomodorosPlanejados { get; set; }
}