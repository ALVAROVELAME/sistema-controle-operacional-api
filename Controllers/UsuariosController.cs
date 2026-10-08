using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaControleOperacionalApi.DTOs.Comum;
using SistemaControleOperacionalApi.DTOs.Usuarios;
using SistemaControleOperacionalApi.Extensions;
using SistemaControleOperacionalApi.Repositories;
using SistemaControleOperacionalApi.Services;

namespace SistemaControleOperacionalApi.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _usuarioService;
    private readonly UsuarioRepository _usuarioRepo;

    public UsuariosController(UsuarioService usuarioService, UsuarioRepository usuarioRepo)
    {
        _usuarioService = usuarioService;
        _usuarioRepo = usuarioRepo;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<MensagemResponseDTO>> Cadastrar([FromBody] UsuarioCadastroDTO dto)
    {
        await _usuarioService.CadastrarAsync(dto);
        return Ok(new MensagemResponseDTO("Cadastro iniciado. Verifique seu email para confirmar a conta."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioResponseDTO>> Me()
    {
        var u = await _usuarioRepo.FindByIdAsync(User.GetUsuarioId());
        if (u is null) return NotFound();
        return Ok(new UsuarioResponseDTO { Id = u.Id, Nome = u.Nome, Email = u.Email, Ativo = u.Ativo });
    }

    [HttpPost("me/excluir")]
    [Authorize]
    public async Task<ActionResult<MensagemResponseDTO>> ExcluirConta([FromBody] ExcluirContaRequestDTO dto)
    {
        var u = await _usuarioRepo.FindByIdAsync(User.GetUsuarioId())
            ?? throw new UnauthorizedAccessException("Usuario nao encontrado.");

        await _usuarioService.ExcluirContaAsync(u, dto.Senha);
        return Ok(new MensagemResponseDTO("Conta excluida com sucesso."));
    }
}