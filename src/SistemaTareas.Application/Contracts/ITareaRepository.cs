using SistemaTareas.Domain.Entities;

namespace SistemaTareas.Application.Contracts;

public interface ITareaRepository
{
    Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Tarea>> ListarAsync(CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}

