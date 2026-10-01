using Microsoft.EntityFrameworkCore;
using SistemaControleOperacionalApi.Models;

namespace SistemaControleOperacionalApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<CadastroPendente> CadastrosPendentes => Set<CadastroPendente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.TokenConfirmacao).IsUnique();
            e.Property(u => u.Nome).IsRequired();
            e.Property(u => u.Email).IsRequired();
            e.Property(u => u.SenhaHash).IsRequired();
            e.Property(u => u.Ativo).IsRequired();
            e.Property(u => u.CriadoEm).IsRequired();
        });

        modelBuilder.Entity<CadastroPendente>(e =>
        {
            e.HasIndex(c => c.Email).IsUnique();
            e.HasIndex(c => c.TokenConfirmacao).IsUnique();
            e.Property(c => c.Nome).IsRequired();
            e.Property(c => c.Email).IsRequired();
            e.Property(c => c.SenhaHash).IsRequired();
            e.Property(c => c.TokenConfirmacao).IsRequired();
            e.Property(c => c.TokenExpiraEm).IsRequired();
            e.Property(c => c.CriadoEm).IsRequired();
        });
    }
}