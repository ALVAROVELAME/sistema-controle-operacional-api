using System.ComponentModel.DataAnnotations;
using SistemaControleOperacionalApi.Enums;

namespace SistemaControleOperacionalApi.DTOs.Pomodoros;

/// <summary>
/// Novo endpoint: POST /api/pomodoros/eventos
/// Substitui o antigo /api/pomodoros/sessoes.
/// Cada transição do timer gera um evento (fire-and-forget no front).
/// </summary>
public class EventoPomodoroRequestDTO
{
    [Required]
    public TipoEventoPomodoro Tipo { get; set; }

    [Required]
    public ModoPomodoro Modo { get; set; }

    [Range(1, 8)]
    public int Ciclo { get; set; }

    [Range(0, 120)]
    public int MinutosPlanejados { get; set; }

    [Range(0, 120)]
    public int MinutosReais { get; set; }

    [Range(0, 7200)]
    public int SegundosReais { get; set; }

    [Required]
    public DateTimeOffset OcorridoEm { get; set; }

    public string? TarefaId { get; set; }
    public string? TarefaTitulo { get; set; }
    public StatusTarefa? StatusTarefa { get; set; }
}