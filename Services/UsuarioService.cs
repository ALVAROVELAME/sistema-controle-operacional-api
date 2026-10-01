using SistemaControleOperacionalApi.DTOs;
using SistemaControleOperacionalApi.Exceptions;
using SistemaControleOperacionalApi.Models;
using SistemaControleOperacionalApi.Repositories;

namespace SistemaControleOperacionalApi.Services;

/// <summary>
/// Espelha UsuarioService (Java).
/// </summary>
public sealed class UsuarioService
{
    private readonly UsuarioRepository _usuarioRepo;
    private readonly CadastroPendenteRepository _cadastroRepo;
    private readonly PasswordHasher _hasher;
    private readonly EmailService _email;

    public UsuarioService(
        UsuarioRepository usuarioRepo,
        CadastroPendenteRepository cadastroRepo,
        PasswordHasher hasher,
        EmailService email)
    {
        _usuarioRepo = usuarioRepo;
        _cadastroRepo = cadastroRepo;
        _hasher = hasher;
        _email = email;
    }

    public async Task<CadastroPendente> CadastrarAsync(UsuarioCadastroDTO dto)
    {
        if (await _usuarioRepo.FindByEmailAsync(dto.Email) is not null)
            throw new ConflictException("Email ja cadastrado");

        if (await _cadastroRepo.FindByEmailAsync(dto.Email) is not null)
            throw new ConflictException("Email aguardando confirmacao");

        var senhaHash = _hasher.Hash(dto.Senha);
        var token = Guid.NewGuid().ToString();

        var cadastro = new CadastroPendente(dto.Nome, dto.Email, senhaHash, token);
        var salvo = await _cadastroRepo.AddAsync(cadastro);

        await _email.EnviarEmailConfirmacaoAsync(
            salvo.Email, salvo.Nome, salvo.TokenConfirmacao);

        return salvo;
    }

    public async Task ExcluirContaAsync(Usuario usuario, string senha)
    {
        // Revalida a senha (mesma protecao do Java)
        if (!_hasher.Verify(senha, usuario.SenhaHash))
            throw new UnauthorizedAccessException(
                "Senha incorreta. Exclusao cancelada.");

        // Remove cadastros pendentes com o mesmo email (se houver)
        var pendente = await _cadastroRepo.FindByEmailAsync(usuario.Email);
        if (pendente is not null)
            await _cadastroRepo.DeleteAsync(pendente);

        // Remove o usuario
        await _usuarioRepo.DeleteAsync(usuario);
    }
}