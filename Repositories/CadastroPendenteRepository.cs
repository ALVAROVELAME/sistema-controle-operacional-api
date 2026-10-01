using Microsoft.EntityFrameworkCore;
using SistemaControleOperacionalApi.Data;
using SistemaControleOperacionalApi.Models;

namespace SistemaControleOperacionalApi.Repositories;

public class CadastroPendenteRepository
{
    private readonly AppDbContext _db;

    public CadastroPendenteRepository(AppDbContext db) => _db = db;

    public Task<CadastroPendente?> FindByTokenAsync(string token)
        => _db.CadastrosPendentes
              .FirstOrDefaultAsync(c => c.TokenConfirmacao == token);

    public Task<CadastroPendente?> FindByEmailAsync(string email)
        => _db.CadastrosPendentes.FirstOrDefaultAsync(c => c.Email == email);

    public async Task<CadastroPendente> AddAsync(CadastroPendente cadastro)
    {
        _db.CadastrosPendentes.Add(cadastro);
        await _db.SaveChangesAsync();
        return cadastro;
    }

    public async Task DeleteAsync(CadastroPendente cadastro)
    {
        _db.CadastrosPendentes.Remove(cadastro);
        await _db.SaveChangesAsync();
    }
}