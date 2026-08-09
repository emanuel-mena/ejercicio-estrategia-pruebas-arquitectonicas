using SistemaTareas.Application.Contracts;
using SistemaTareas.Application.Exceptions;
using SistemaTareas.Domain.Exceptions;

namespace SistemaTareas.Application.UseCases.AsignarTarea;

public sealed class AsignarTareaUseCase(
    ITareaRepository tareas,
    IUsuarioRepository usuarios,
    TimeProvider timeProvider) : IAsignarTareaUseCase
{
    public async Task<AsignarTareaResult> ExecuteAsync(
        AsignarTareaCommand command,
        CancellationToken cancellationToken)
    {
        var tarea = await tareas.ObtenerPorIdAsync(command.TareaId, cancellationToken);
        if (tarea is null)
        {
            return new(CodigoAsignacion.TareaNoEncontrada, "La tarea no existe.");
        }

        var usuario = await usuarios.ObtenerPorIdAsync(command.UsuarioId, cancellationToken);
        if (usuario is null)
        {
            return new(CodigoAsignacion.UsuarioNoEncontrado, "El usuario no existe.");
        }

        try
        {
            tarea.AsignarA(usuario, timeProvider.GetUtcNow());
            await tareas.GuardarCambiosAsync(cancellationToken);
            return new(CodigoAsignacion.Exito, $"La tarea se asignó correctamente a {usuario.Nombre}.");
        }
        catch (ReglaDominioException exception)
        {
            return exception.Codigo switch
            {
                "USUARIO_INACTIVO" => new(CodigoAsignacion.UsuarioInactivo, exception.Message),
                "TAREA_YA_ASIGNADA" => new(CodigoAsignacion.TareaYaAsignada, exception.Message),
                _ => new(CodigoAsignacion.EstadoInvalido, exception.Message)
            };
        }
        catch (ConflictoConcurrenciaException)
        {
            return new(
                CodigoAsignacion.ConflictoConcurrencia,
                "La tarea fue modificada por otra persona. Actualice la página e inténtelo nuevamente.");
        }
    }
}

