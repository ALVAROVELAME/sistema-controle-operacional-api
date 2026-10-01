using Microsoft.EntityFrameworkCore;
using SistemaControleOperacionalApi.Data;
using SistemaControleOperacionalApi.Models;

namespace SistemaControleOperacionalApi.Repositories;

/// <summary>
/// Espelha UsuarioRepository (Spring Data JPA).
/// </summary>
public class UsuarioRepository
{
    private readonly AppDbContext _db;

    public UsuarioRepository(AppDbContext db) => _db = db;

    public Task<Usuario?> FindByIdAsync(long id)
        => _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

    public Task<Usuario?> FindByEmailAsync(string email)
        => _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

    public Task<Usuario?> FindByTokenConfirmacaoAsync(string token)
        => _db.Usuarios.FirstOrDefaultAsync(u => u.TokenConfirmacao == token);

    public async Task<Usuario> AddAsync(Usuario usuario)
    {
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        return usuario;
    }

    public async Task DeleteAsync(Usuario usuario)
    {
        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();
    }
}