using SistemaControleOperacionalApi.Data;
using SistemaControleOperacionalApi.Models;

namespace SistemaControleOperacionalApi.Repositories;

public class PomodoroEventoRepository
{
    private readonly AppDbContext _db;
    public PomodoroEventoRepository(AppDbContext db) => _db = db;

    public async Task<PomodoroEvento> AddAsync(PomodoroEvento e)
    {
        _db.PomodoroEventos.Add(e);
        await _db.SaveChangesAsync();
        return e;
    }
}