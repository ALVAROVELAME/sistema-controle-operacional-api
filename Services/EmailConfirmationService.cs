using SistemaControleOperacionalApi.Models;
using SistemaControleOperacionalApi.Repositories;

namespace SistemaControleOperacionalApi.Services;

/// <summary>
/// Espelha EmailConfirmationService (Java).
/// </summary>
public sealed class EmailConfirmationService
{
    private readonly CadastroPendenteRepository _cadastroRepo;
    private readonly UsuarioRepository _usuarioRepo;

    public EmailConfirmationService(
        CadastroPendenteRepository cadastroRepo,
        UsuarioRepository usuarioRepo)
    {
        _cadastroRepo = cadastroRepo;
        _usuarioRepo = usuarioRepo;
    }

    public async Task<bool> ConfirmarEmailAsync(string token)
    {
        var cadastro = await _cadastroRepo.FindByTokenAsync(token);
        if (cadastro is null) return false;

        if (cadastro.TokenExpiraEm < DateTime.UtcNow) return false;

        var usuario = new Usuario
        {
            Nome = cadastro.Nome,
            Email = cadastro.Email,
            SenhaHash = cadastro.SenhaHash,
            CriadoEm = DateTime.UtcNow
        };
        usuario.AtivarConta();

        await _usuarioRepo.AddAsync(usuario);
        await _cadastroRepo.DeleteAsync(cadastro);

        return true;
    }
}