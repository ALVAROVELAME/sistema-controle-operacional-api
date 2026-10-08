using System.ComponentModel.DataAnnotations;

namespace SistemaControleOperacionalApi.DTOs.Auth;

public class LoginRequestDTO
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Senha { get; set; } = string.Empty;
}