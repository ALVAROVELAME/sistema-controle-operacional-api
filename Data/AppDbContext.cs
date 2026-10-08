using Microsoft.EntityFrameworkCore;
using SistemaControleOperacionalApi.Models;

namespace SistemaControleOperacionalApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<CadastroPendente> CadastrosPendentes => Set<CadastroPendente>();
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
    public DbSet<PomodoroEvento> PomodoroEventos => Set<PomodoroEvento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.TokenConfirmacao).IsUnique();
            e.Property(u => u.Nome).IsRequired().HasMaxLength(150);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.Property(u => u.SenhaHash).IsRequired();
        });

        modelBuilder.Entity<CadastroPendente>(e =>
        {
            e.HasIndex(c => c.Email).IsUnique();
            e.HasIndex(c => c.TokenConfirmacao).IsUnique();
            e.Property(c => c.Nome).IsRequired().HasMaxLength(150);
            e.Property(c => c.Email).IsRequired().HasMaxLength(200);
            e.Property(c => c.SenhaHash).IsRequired();
            e.Property(c => c.TokenConfirmacao).IsRequired();
        });

        modelBuilder.Entity<Tarefa>(e =>
        {
            e.HasIndex(t => t.UsuarioId);
            e.Property(t => t.Titulo).IsRequired().HasMaxLength(200);
            e.Property(t => t.Descricao).HasMaxLength(5000);
        });

        modelBuilder.Entity<PomodoroEvento>(e =>
        {
            e.HasIndex(p => p.UsuarioId);
        });
    }
}