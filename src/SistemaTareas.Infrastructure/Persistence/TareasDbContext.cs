using Microsoft.EntityFrameworkCore;
using SistemaTareas.Domain.Entities;

namespace SistemaTareas.Infrastructure.Persistence;

public sealed class TareasDbContext(DbContextOptions<TareasDbContext> options) : DbContext(options)
{
    public DbSet<Tarea> Tareas => Set<Tarea>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TareasDbContext).Assembly);
    }
}

