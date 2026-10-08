using System.ComponentModel.DataAnnotations;

namespace SistemaControleOperacionalApi.DTOs.Usuarios;

public class UsuarioCadastroDTO
{
    [Required, MinLength(3), MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Senha { get; set; } = string.Empty;
}