using System.Security.Claims;

namespace SistemaControleOperacionalApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long GetUsuarioId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirst("id")?.Value
            ?? throw new UnauthorizedAccessException("Claim 'id' ausente no token.");
        return long.Parse(raw);
    }

    public static string GetEmail(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? user.FindFirst("sub")?.Value
           ?? throw new UnauthorizedAccessException("Claim de e-mail ausente.");

    public static string GetNome(this ClaimsPrincipal user)
        => user.FindFirst("nome")?.Value ?? string.Empty;
}