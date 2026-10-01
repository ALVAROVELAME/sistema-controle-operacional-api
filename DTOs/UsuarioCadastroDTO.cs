using System.ComponentModel.DataAnnotations;

namespace SistemaControleOperacionalApi.DTOs;

public sealed class UsuarioCadastroDTO
{
    [Required(ErrorMessage = "Nome e obrigatorio")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail e obrigatorio")]
    [EmailAddress(ErrorMessage = "E-mail invalido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha e obrigatoria")]
    [StringLength(100, MinimumLength = 6,
        ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
    public string Senha { get; set; } = string.Empty;
}