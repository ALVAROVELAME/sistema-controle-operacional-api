using SistemaControleOperacionalApi.DTOs;
using SistemaControleOperacionalApi.Exceptions;
using SistemaControleOperacionalApi.Models;
using SistemaControleOperacionalApi.Repositories;

namespace SistemaControleOperacionalApi.Services;

/// <summary>
/// Espelha AuthService (Java).
/// </summary>
public sealed class AuthService
{
    private readonly UsuarioRepository _usuarioRepo;
    private readonly PasswordHasher _hasher;

    public AuthService(UsuarioRepository usuarioRepo, PasswordHasher hasher)
    {
        _usuarioRepo = usuarioRepo;
        _hasher = hasher;
    }

    public async Task<Usuario> AutenticarAsync(LoginDTO dto)
    {
        var usuario = await _usuarioRepo.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("E-mail ou senha invalidos.");

        if (!usuario.Ativo)
            throw new ForbiddenException(
                "Conta ainda nao foi confirmada. Verifique seu e-mail.");

        if (!_hasher.Verify(dto.Senha, usuario.SenhaHash))
            throw new UnauthorizedAccessException("E-mail ou senha invalidos.");

        return usuario;
    }
}