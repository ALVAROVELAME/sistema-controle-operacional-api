using System.ComponentModel.DataAnnotations;
namespace SistemaControleOperacionalApi.DTOs.Auth;
public class RecuperarSenhaDTO
{
    [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = string.Empty;
}
public class RedefinirSenhaDTO
{
    [Required, MaxLength(128)] public string Token { get; set; } = string.Empty;
    [Required, MinLength(6), MaxLength(100)] public string Senha { get; set; } = string.Empty;
}
