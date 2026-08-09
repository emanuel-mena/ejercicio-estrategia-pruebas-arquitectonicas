using SistemaTareas.Domain.Entities;

namespace SistemaTareas.Application.Contracts;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Usuario>> ListarActivosAsync(CancellationToken cancellationToken);
}

