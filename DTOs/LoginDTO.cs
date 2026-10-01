using System.ComponentModel.DataAnnotations;

namespace SistemaControleOperacionalApi.DTOs;

/// <summary>
/// Espelha LoginDTO (Java) com Bean Validation -> DataAnnotations.
/// </summary>
public sealed class LoginDTO
{
    [Required(ErrorMessage = "E-mail e obrigatorio")]
    [EmailAddress(ErrorMessage = "E-mail invalido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha e obrigatoria")]
    public string Senha { get; set; } = string.Empty;
}