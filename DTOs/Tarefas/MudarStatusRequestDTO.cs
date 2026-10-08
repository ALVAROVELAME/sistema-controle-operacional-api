using SistemaControleOperacionalApi.Enums;

namespace SistemaControleOperacionalApi.DTOs.Tarefas;

public class MudarStatusRequestDTO
{
    public StatusTarefa Status { get; set; }
}