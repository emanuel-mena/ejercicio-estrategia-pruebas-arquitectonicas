namespace SistemaTareas.Application.Queries;

public interface IConsultaTareas
{
    Task<IReadOnlyList<TareaResumen>> ListarTareasAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<UsuarioResumen>> ListarUsuariosActivosAsync(CancellationToken cancellationToken);
}

