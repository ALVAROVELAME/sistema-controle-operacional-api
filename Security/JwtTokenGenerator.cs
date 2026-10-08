using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SistemaControleOperacionalApi.Config;

namespace SistemaControleOperacionalApi.Security;

public sealed class JwtTokenGenerator
{
    private readonly JwtSettings _settings;
    private readonly SigningCredentials _credentials;

    public JwtTokenGenerator(JwtSettings settings)
    {
        _settings = settings;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public string GerarToken(long usuarioId, string email, string nome)
    {
        var agora = DateTime.UtcNow;
        var expiraEm = agora.AddMilliseconds(_settings.Expiration);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, email),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(agora).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new("id", usuarioId.ToString()),
            new("nome", nome),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,   // 👈 FIX
            claims: claims,
            notBefore: agora,
            expires: expiraEm,
            signingCredentials: _credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}