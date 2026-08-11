using Microsoft.EntityFrameworkCore;
using SistemaTareas.Application.Contracts;
using SistemaTareas.Application.Exceptions;
using SistemaTareas.Domain.Entities;
using SistemaTareas.Infrastructure.Persistence;

namespace SistemaTareas.Infrastructure.Repositories;

public sealed class TareaRepository(TareasDbContext dbContext) : ITareaRepository
{
    public async Task AgregarAsync(Tarea tarea, CancellationToken cancellationToken)
    {
        await dbContext.Tareas.AddAsync(tarea, cancellationToken);
    }

    public Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Tareas.SingleOrDefaultAsync(tarea => tarea.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Tarea>> ListarAsync(CancellationToken cancellationToken) =>
        await dbContext.Tareas.OrderBy(tarea => tarea.Titulo).ToListAsync(cancellationToken);

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConflictoConcurrenciaException("La tarea fue modificada concurrentemente.", exception);
        }
    }
}
