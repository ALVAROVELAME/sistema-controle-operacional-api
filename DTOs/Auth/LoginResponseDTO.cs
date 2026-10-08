using SistemaControleOperacionalApi.DTOs.Usuarios;

namespace SistemaControleOperacionalApi.DTOs.Auth;

public class LoginResponseDTO
{
    public bool Sucesso { get; set; } = true;
    public string? Mensagem { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Bearer";
    public UsuarioResponseDTO? Usuario { get; set; }
}