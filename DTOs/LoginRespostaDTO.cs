namespace SistemaControleOperacionalApi.DTOs;

/// <summary>
/// Espelha LoginRespostaDTO (Java).
/// </summary>
public sealed record LoginRespostaDTO(
    bool Sucesso,
    string Mensagem,
    string? Token = null,
    UsuarioRespostaDTO? Usuario = null
);