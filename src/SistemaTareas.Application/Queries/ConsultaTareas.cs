using SistemaTareas.Application.Contracts;

namespace SistemaTareas.Application.Queries;

public sealed class ConsultaTareas(
    ITareaRepository tareas,
    IUsuarioRepository usuarios) : IConsultaTareas
{
    public async Task<IReadOnlyList<TareaResumen>> ListarTareasAsync(CancellationToken cancellationToken)
    {
        var usuariosActivos = await usuarios.ListarActivosAsync(cancellationToken);
        var nombres = usuariosActivos.ToDictionary(usuario => usuario.Id, usuario => usuario.Nombre);
        var entidades = await tareas.ListarAsync(cancellationToken);

        return entidades
            .Select(tarea => new TareaResumen(
                tarea.Id,
                tarea.Titulo,
                tarea.Descripcion,
                tarea.Estado.ToString(),
                tarea.UsuarioAsignadoId,
                tarea.UsuarioAsignadoId is { } id && nombres.TryGetValue(id, out var nombre) ? nombre : null,
                tarea.AsignadaEn))
            .ToArray();
    }

    public async Task<IReadOnlyList<UsuarioResumen>> ListarUsuariosActivosAsync(
        CancellationToken cancellationToken)
    {
        var entidades = await usuarios.ListarActivosAsync(cancellationToken);
        return entidades.Select(usuario => new UsuarioResumen(usuario.Id, usuario.Nombre)).ToArray();
    }
}

