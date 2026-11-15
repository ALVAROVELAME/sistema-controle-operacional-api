using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SistemaControleOperacionalApi.Config;
using SistemaControleOperacionalApi.Security;
using SistemaControleOperacionalApi.Tests.Infrastructure;

namespace SistemaControleOperacionalApi.Tests.Unit;

public sealed class JwtTests
{
    [Fact]
    public void Token_tem_assinatura_claims_audiencia_e_expiracao_validas()
    {
        var settings = new JwtSettings { Secret = ApiFactory.Secret, Issuer = "issuer", Audience = "audience", Expiration = 60000 };
        var token = new JwtTokenGenerator(settings).GerarToken(42, "user@example.test", "Maria");
        var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = settings.Issuer,
            ValidateAudience = true, ValidAudience = settings.Audience,
            ValidateLifetime = true, ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)),
            ClockSkew = TimeSpan.Zero
        }, out var validated);
        principal.FindFirst("id")!.Value.Should().Be("42");
        principal.FindFirst("nome")!.Value.Should().Be("Maria");
        validated.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(1), TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("curta")]
    public void Configuracao_exige_segredo_suficiente(string? secret)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["JWT_SECRET"] = secret }).Build();
        var action = () => JwtSettings.FromConfiguration(configuration);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Configuracao_usa_valores_padrao_documentados()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["JWT_SECRET"] = ApiFactory.Secret }).Build();
        var settings = JwtSettings.FromConfiguration(configuration);
        settings.Expiration.Should().Be(86400000);
        settings.Audience.Should().Be(settings.Issuer);
    }
}
