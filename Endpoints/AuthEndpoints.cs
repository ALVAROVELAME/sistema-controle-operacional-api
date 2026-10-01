using SistemaControleOperacionalApi.DTOs;
using SistemaControleOperacionalApi.Extensions;
using SistemaControleOperacionalApi.Repositories;
using SistemaControleOperacionalApi.Security;
using SistemaControleOperacionalApi.Services;

namespace SistemaControleOperacionalApi.Endpoints;

/// <summary>
/// Espelha AuthController (Java).
/// </summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        // ---------- POST /api/auth/login - publico ----------
        group.MapPost("/login", async (
            LoginDTO dto,
            AuthService authService,
            JwtTokenGenerator jwtGen) =>
        {
            var usuario = await authService.AutenticarAsync(dto);
            var token = jwtGen.GerarToken(usuario.Id, usuario.Email, usuario.Nome);

            return Results.Ok(new LoginRespostaDTO(
                Sucesso: true,
                Mensagem: "Login realizado com sucesso",
                Token: token,
                Usuario: UsuarioRespostaDTO.FromUsuario(usuario)));
        })
        .AddEndpointFilter<ValidationFilter<LoginDTO>>()
        .AllowAnonymous();

        // ---------- GET /api/auth/me - protegido ----------
        group.MapGet("/me", async (HttpContext http, UsuarioRepository repo) =>
        {
            var usuarioId = http.User.GetUsuarioId();
            var usuario = await repo.FindByIdAsync(usuarioId)
                ?? throw new UnauthorizedAccessException("Usuario nao encontrado.");

            return Results.Ok(UsuarioRespostaDTO.FromUsuario(usuario));
        })
        .RequireAuthorization();

        // ---------- GET /api/auth/confirmar?token=... - publico ----------
        group.MapGet("/confirmar", async (
            string token,
            EmailConfirmationService service) =>
        {
            var confirmado = await service.ConfirmarEmailAsync(token);

            return confirmado
                ? Results.Ok(new LoginRespostaDTO(true, "E-mail confirmado com sucesso"))
                : Results.BadRequest(new LoginRespostaDTO(false, "Token invalido ou expirado"));
        })
        .AllowAnonymous();

        return app;
    }
}