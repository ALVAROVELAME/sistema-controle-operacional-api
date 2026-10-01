using SistemaControleOperacionalApi.Models;

namespace SistemaControleOperacionalApi.DTOs;

/// <summary>
/// Espelha UsuarioRespostaDTO (Java).
/// </summary>
public sealed record UsuarioRespostaDTO(
    long Id,
    string Nome,
    string Email,
    bool Ativo)
{
    public static UsuarioRespostaDTO FromUsuario(Usuario u)
        => new(u.Id, u.Nome, u.Email, u.Ativo);
}