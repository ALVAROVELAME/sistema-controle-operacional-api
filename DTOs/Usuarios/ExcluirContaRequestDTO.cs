using System.ComponentModel.DataAnnotations;

namespace SistemaControleOperacionalApi.DTOs.Usuarios;

/// <summary>
/// Novo endpoint: POST /api/usuarios/me/excluir
/// Substitui o antigo DELETE com body (mal suportado por proxies).
/// </summary>
public class ExcluirContaRequestDTO
{
    [Required, MinLength(6)]
    public string Senha { get; set; } = string.Empty;
}