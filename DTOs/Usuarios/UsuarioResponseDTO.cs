namespace SistemaControleOperacionalApi.DTOs.Usuarios;

public class UsuarioResponseDTO
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}