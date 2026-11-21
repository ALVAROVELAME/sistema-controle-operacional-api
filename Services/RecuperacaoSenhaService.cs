using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SistemaControleOperacionalApi.Data;

namespace SistemaControleOperacionalApi.Services;

public sealed class RecuperacaoSenhaService(AppDbContext db, IEmailService email, PasswordHasher hasher, ILogger<RecuperacaoSenhaService> logger)
{
    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public async Task SolicitarAsync(string endereco)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == endereco && u.Ativo);
        if (usuario is null) return;
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        usuario.RecuperacaoTokenHash = Hash(token);
        usuario.RecuperacaoExpiraEm = DateTime.UtcNow.AddMinutes(30);
        await db.SaveChangesAsync();
        try { await email.EnviarEmailRecuperacaoAsync(usuario.Email, usuario.Nome, token); }
        catch (Exception ex) { logger.LogError(ex, "Falha ao enviar e-mail de recuperacao"); }
    }

    public async Task<bool> RedefinirAsync(string token, string senha)
    {
        var hash = Hash(token);
        var agora = DateTime.UtcNow;
        var senhaHash = hasher.Hash(senha);
        // Atualizacao condicional atomica impede que duas requisicoes usem o mesmo token.
        var atualizados = await db.Usuarios.Where(u => u.Ativo && u.RecuperacaoTokenHash == hash && u.RecuperacaoExpiraEm > agora)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.SenhaHash, senhaHash)
                .SetProperty(u => u.RecuperacaoTokenHash, (string?)null)
                .SetProperty(u => u.RecuperacaoExpiraEm, (DateTime?)null));
        return atualizados == 1;
    }
}
