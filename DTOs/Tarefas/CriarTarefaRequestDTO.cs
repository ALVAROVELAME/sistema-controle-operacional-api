using System.ComponentModel.DataAnnotations;
using SistemaControleOperacionalApi.Enums;

namespace SistemaControleOperacionalApi.DTOs.Tarefas;

public class CriarTarefaRequestDTO
{
    [Required, MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Descricao { get; set; }

    public Prioridade Prioridade { get; set; } = Prioridade.Media;

    public StatusTarefa Status { get; set; } = StatusTarefa.AFazer;

    /// <summary>YYYY-MM-DD ou null.</summary>
    public DateOnly? Prazo { get; set; }

    /// <summary>Estimativa (1–20). null = sem estimativa.</summary>
    [Range(1, 20)]
    public int? PomodorosPlanejados { get; set; }

    /// <summary>Sempre 0 no create — o contador pertence ao servidor.</summary>
    [Range(0, int.MaxValue)]
    public int Pomodoros { get; set; } = 0;
}