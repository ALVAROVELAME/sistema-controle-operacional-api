using SistemaControleOperacionalApi.Enums;

namespace SistemaControleOperacionalApi.DTOs.Tarefas;

public class TarefaResponseDTO
{
    public string Id { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Prioridade Prioridade { get; set; }
    public StatusTarefa Status { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
    public DateOnly? Prazo { get; set; }
    public int? Pomodoros { get; set; }
    public int? PomodorosPlanejados { get; set; }
}