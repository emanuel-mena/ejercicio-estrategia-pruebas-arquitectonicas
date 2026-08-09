using Microsoft.EntityFrameworkCore;
using SistemaTareas.Application.Contracts;
using SistemaTareas.Domain.Entities;
using SistemaTareas.Infrastructure.Persistence;

namespace SistemaTareas.Infrastructure.Repositories;

public sealed class UsuarioRepository(TareasDbContext dbContext) : IUsuarioRepository
{
    public Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Usuarios.SingleOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListarActivosAsync(CancellationToken cancellationToken) =>
        await dbContext.Usuarios
            .Where(usuario => usuario.Activo)
            .OrderBy(usuario => usuario.Nombre)
            .ToListAsync(cancellationToken);
}

