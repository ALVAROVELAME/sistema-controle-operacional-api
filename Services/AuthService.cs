using SistemaControleOperacionalApi.DTOs.Auth;
using SistemaControleOperacionalApi.DTOs.Usuarios;
using SistemaControleOperacionalApi.Repositories;
using SistemaControleOperacionalApi.Security;

namespace SistemaControleOperacionalApi.Services;

public sealed class AuthService
{
    private readonly UsuarioRepository _usuarioRepo;
    private readonly PasswordHasher _hasher;
    private readonly JwtTokenGenerator _jwt;

    public AuthService(UsuarioRepository usuarioRepo, PasswordHasher hasher, JwtTokenGenerator jwt)
    {
        _usuarioRepo = usuarioRepo;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto)
    {
        var usuario = await _usuarioRepo.FindByEmailAsync(dto.Email);

        if (usuario is null || !_hasher.Verify(dto.Senha, usuario.SenhaHash))
            return new LoginResponseDTO { Sucesso = false, Mensagem = "E-mail ou senha invalidos." };

        if (!usuario.Ativo)
            return new LoginResponseDTO { Sucesso = false, Mensagem = "Conta ainda nao foi confirmada." };

        var token = _jwt.GerarToken(usuario.Id, usuario.Email, usuario.Nome);

        return new LoginResponseDTO
        {
            Sucesso = true,
            Mensagem = "Login realizado com sucesso",
            Token = token,
            Usuario = ToDto(usuario),
        };
    }

    public async Task<UsuarioResponseDTO?> ObterPorIdAsync(long id)
    {
        var u = await _usuarioRepo.FindByIdAsync(id);
        return u is null ? null : ToDto(u);
    }

    private static UsuarioResponseDTO ToDto(Models.Usuario u) => new()
    {
        Id = u.Id,
        Nome = u.Nome,
        Email = u.Email,
        Ativo = u.Ativo,
    };
}