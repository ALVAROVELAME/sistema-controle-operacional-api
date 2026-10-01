using System.ComponentModel.DataAnnotations;

namespace SistemaControleOperacionalApi.DTOs;

public sealed class ExcluirContaDTO
{
    [Required(ErrorMessage = "Senha e obrigatoria para confirmar a exclusao")]
    public string Senha { get; set; } = string.Empty;
}