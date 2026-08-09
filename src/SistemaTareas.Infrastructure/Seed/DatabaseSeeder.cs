using Microsoft.EntityFrameworkCore;
using SistemaTareas.Domain.Entities;
using SistemaTareas.Infrastructure.Persistence;

namespace SistemaTareas.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task InitializeAsync(
        TareasDbContext dbContext,
        int extraPendingTasks = 0,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (!await dbContext.Usuarios.AnyAsync(cancellationToken))
        {
            await dbContext.Usuarios.AddRangeAsync(SeedData.Usuarios, cancellationToken);
        }

        if (!await dbContext.Tareas.AnyAsync(cancellationToken))
        {
            await dbContext.Tareas.AddRangeAsync(SeedData.Tareas, cancellationToken);

            if (extraPendingTasks > 0)
            {
                var tareasCarga = Enumerable.Range(1, extraPendingTasks)
                    .Select(index => new Tarea(
                        Guid.NewGuid(),
                        $"Tarea de carga {index:D5}",
                        "Dato generado para la prueba de rendimiento."));
                await dbContext.Tareas.AddRangeAsync(tareasCarga, cancellationToken);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

