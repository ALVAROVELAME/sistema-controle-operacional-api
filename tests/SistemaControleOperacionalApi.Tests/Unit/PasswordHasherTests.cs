using SistemaControleOperacionalApi.Services;

namespace SistemaControleOperacionalApi.Tests.Unit;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_usa_salt_e_verifica_apenas_senha_correta()
    {
        const string password = "MinhaSenha123!";
        var hash = _hasher.Hash(password);
        hash.Should().NotBe(password).And.NotBe(_hasher.Hash(password));
        _hasher.Verify(password, hash).Should().BeTrue();
        _hasher.Verify("OutraSenha", hash).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Hash_rejeita_senha_vazia(string? password)
    {
        var action = () => _hasher.Hash(password!);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("", "hash")]
    [InlineData("senha", "")]
    [InlineData("senha", "hash-invalido")]
    [InlineData(null, null)]
    public void Verify_recusa_dados_invalidos(string? password, string? hash)
        => _hasher.Verify(password!, hash!).Should().BeFalse();
}
