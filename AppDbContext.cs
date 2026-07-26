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

}