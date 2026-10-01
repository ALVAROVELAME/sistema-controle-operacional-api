namespace SistemaControleOperacionalApi.Models;

/// <summary>
/// Espelha a entidade Usuario (Java).
/// </summary>
public class Usuario
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public bool Ativo { get; set; } = false;
    public string? TokenConfirmacao { get; set; }
    public DateTime? TokenExpiraEm { get; set; }
    public DateTime? EmailConfirmadoEm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public void AtivarConta()
    {
        Ativo = true;
        EmailConfirmadoEm = DateTime.UtcNow;
        TokenConfirmacao = null;
        TokenExpiraEm = null;
    }
}