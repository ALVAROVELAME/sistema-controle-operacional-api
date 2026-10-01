using SistemaControleOperacionalApi.DTOs;
using SistemaControleOperacionalApi.Extensions;
using SistemaControleOperacionalApi.Repositories;
using SistemaControleOperacionalApi.Services;

namespace SistemaControleOperacionalApi.Endpoints;

/// <summary>
/// Espelha UsuarioController (Java).
/// </summary>
public static class UsuarioEndpoints
{
    public static IEndpointRouteBuilder MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/usuarios");

        // ---------- POST /api/usuarios - publico ----------
        group.MapPost("", async (
            UsuarioCadastroDTO dto,
            UsuarioService service) =>
        {
            await service.CadastrarAsync(dto);

            return Results.Ok(new MensagemRespostaDTO(
                true,
                "Cadastro iniciado. Verifique seu email para confirmar a conta."));
        })
        .AddEndpointFilter<ValidationFilter<UsuarioCadastroDTO>>()
        .AllowAnonymous();

        // ---------- DELETE /api/usuarios/me - protegido ----------
        group.MapDelete("/me", async (
            HttpContext http,
            ExcluirContaDTO dto,
            UsuarioRepository repo,
            UsuarioService service) =>
        {
            var usuarioId = http.User.GetUsuarioId();
            var usuario = await repo.FindByIdAsync(usuarioId)
                ?? throw new UnauthorizedAccessException("Usuario nao encontrado.");

            await service.ExcluirContaAsync(usuario, dto.Senha);

            return Results.Ok(new MensagemRespostaDTO(true, "Conta excluida com sucesso."));
        })
        .AddEndpointFilter<ValidationFilter<ExcluirContaDTO>>()
        .RequireAuthorization();

        return app;
    }
}