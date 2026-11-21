using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaControleOperacionalApi.DTOs.Auth;
using SistemaControleOperacionalApi.DTOs.Comum;
using SistemaControleOperacionalApi.DTOs.Usuarios;
using SistemaControleOperacionalApi.Extensions;
using SistemaControleOperacionalApi.Services;

namespace SistemaControleOperacionalApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly EmailConfirmationService _confirmacao;

    public AuthController(AuthService authService, EmailConfirmationService confirmacao)
    {
        _authService = authService;
        _confirmacao = confirmacao;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDTO dto)
    {
        var resposta = await _authService.LoginAsync(dto);
        return resposta.Sucesso ? Ok(resposta) : Unauthorized(resposta);
    }

    [HttpPost("recuperar-senha")]
    [AllowAnonymous]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("recuperacao")]
    public async Task<ActionResult<MensagemResponseDTO>> RecuperarSenha([FromBody] RecuperarSenhaDTO dto, [FromServices] RecuperacaoSenhaService service)
    {
        await service.SolicitarAsync(dto.Email);
        return Ok(new MensagemResponseDTO("Se houver uma conta com este e-mail, enviaremos as instrucoes."));
    }

    [HttpPost("redefinir-senha")]
    [AllowAnonymous]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("recuperacao")]
    public async Task<ActionResult<MensagemResponseDTO>> RedefinirSenha([FromBody] RedefinirSenhaDTO dto, [FromServices] RecuperacaoSenhaService service)
    {
        if (string.IsNullOrWhiteSpace(dto.Senha))
            return BadRequest(new MensagemResponseDTO("A senha nao pode ficar em branco.", false));
        if (!await service.RedefinirAsync(dto.Token, dto.Senha))
            return BadRequest(new MensagemResponseDTO("Link invalido ou expirado. Solicite um novo link.", false));
        return Ok(new MensagemResponseDTO("Senha redefinida com sucesso."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioResponseDTO>> Me()
    {
        var usuario = await _authService.ObterPorIdAsync(User.GetUsuarioId());
        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpGet("confirmar")]
    [AllowAnonymous]
    public async Task<ActionResult<MensagemResponseDTO>> Confirmar([FromQuery] string token)
    {
        var ok = await _confirmacao.ConfirmarEmailAsync(token);
        if (!ok) return BadRequest(new MensagemResponseDTO("Token invalido ou expirado", false));
        return Ok(new MensagemResponseDTO("E-mail confirmado com sucesso"));
    }
}