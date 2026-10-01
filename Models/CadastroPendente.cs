namespace SistemaControleOperacionalApi.Models;

/// <summary>
/// Espelha a entidade CadastroPendente (Java).
/// Registro temporario ate o e-mail ser confirmado.
/// </summary>
public class CadastroPendente
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string TokenConfirmacao { get; set; } = string.Empty;
    public DateTime TokenExpiraEm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public CadastroPendente() { }

    public CadastroPendente(
        string nome,
        string email,
        string senhaHash,
        string tokenConfirmacao)
    {
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        TokenConfirmacao = tokenConfirmacao;
        TokenExpiraEm = DateTime.UtcNow.AddHours(24);
        CriadoEm = DateTime.UtcNow;
    }
}