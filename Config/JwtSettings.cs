namespace SistemaControleOperacionalApi.Config;

public sealed class JwtSettings
{
    public string Secret { get; init; } = string.Empty;
    public long Expiration { get; init; } = 86_400_000;
    public string Issuer { get; init; } = "sistema-controle-operacional-api";
    public string Audience { get; init; } = "sistema-controle-operacional-api";

    public static JwtSettings FromEnvironment()
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("JWT_SECRET nao configurado no ambiente.");

        if (secret.Length < 32)
            throw new InvalidOperationException("JWT_SECRET deve ter pelo menos 32 caracteres.");

        var expirationRaw = Environment.GetEnvironmentVariable("JWT_EXPIRATION");
        var expiration = long.TryParse(expirationRaw, out var parsed) ? parsed : 86_400_000L;

        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? "sistema-controle-operacional-api";

        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? issuer;

        return new JwtSettings
        {
            Secret = secret,
            Expiration = expiration,
            Issuer = issuer,
            Audience = audience,
        };
    }
}