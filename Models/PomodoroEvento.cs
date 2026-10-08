using SistemaControleOperacionalApi.Enums;

namespace SistemaControleOperacionalApi.Models;

public class PomodoroEvento
{
    public long Id { get; set; }
    public long UsuarioId { get; set; }
    public TipoEventoPomodoro Tipo { get; set; }
    public ModoPomodoro Modo { get; set; }
    public int Ciclo { get; set; }
    public int MinutosPlanejados { get; set; }
    public int MinutosReais { get; set; }
    public int SegundosReais { get; set; }
    public DateTimeOffset OcorridoEm { get; set; }
    public long? TarefaId { get; set; }
    public string? TarefaTitulo { get; set; }
    public StatusTarefa? StatusTarefa { get; set; }
}