using Microsoft.EntityFrameworkCore;
using SistemaControleOperacionalApi.Data;
using SistemaControleOperacionalApi.Models;

namespace SistemaControleOperacionalApi.Repositories;

public class TarefaRepository
{
    private readonly AppDbContext _db;
    public TarefaRepository(AppDbContext db) => _db = db;

    public Task<List<Tarefa>> ListarPorUsuarioAsync(long usuarioId)
        => _db.Tarefas.Where(t => t.UsuarioId == usuarioId)
                      .OrderBy(t => t.CriadoEm)
                      .ToListAsync();

    public Task<Tarefa?> FindByIdAsync(long id, long usuarioId)
        => _db.Tarefas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioId);

    public async Task<Tarefa> AddAsync(Tarefa t)
    {
        _db.Tarefas.Add(t);
        await _db.SaveChangesAsync();
        return t;
    }

    public async Task UpdateAsync(Tarefa t)
    {
        _db.Tarefas.Update(t);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Tarefa t)
    {
        _db.Tarefas.Remove(t);
        await _db.SaveChangesAsync();
    }

    public async Task ZerarAsync(long usuarioId)
    {
        var tarefas = await _db.Tarefas.Where(t => t.UsuarioId == usuarioId).ToListAsync();
        _db.Tarefas.RemoveRange(tarefas);
        await _db.SaveChangesAsync();
    }
}