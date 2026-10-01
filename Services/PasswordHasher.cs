namespace SistemaControleOperacionalApi.Services;

/// <summary>
/// Espelha o BCryptPasswordEncoder do Spring Security.
/// Hash de senhas com BCrypt (work factor padrao = 11).
/// </summary>
public sealed class PasswordHasher
{
    public string Hash(string senhaPlana)
    {
        if (string.IsNullOrWhiteSpace(senhaPlana))
            throw new ArgumentException("Senha nao pode ser vazia.", nameof(senhaPlana));

        return BCrypt.Net.BCrypt.HashPassword(senhaPlana);
    }

    public bool Verify(string senhaPlana, string hashArmazenado)
    {
        if (string.IsNullOrWhiteSpace(senhaPlana) ||
            string.IsNullOrWhiteSpace(hashArmazenado))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(senhaPlana, hashArmazenado);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}