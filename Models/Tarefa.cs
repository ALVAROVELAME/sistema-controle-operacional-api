using SistemaControleOperacionalApi.Enums;

namespace SistemaControleOperacionalApi.Models;

public class Tarefa
{
    public long Id { get; set; }
    public long UsuarioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Prioridade Prioridade { get; set; } = Prioridade.Media;
    public StatusTarefa Status { get; set; } = StatusTarefa.AFazer;
    public DateOnly? Prazo { get; set; }
    public int Pomodoros { get; set; }
    public int? PomodorosPlanejados { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
}