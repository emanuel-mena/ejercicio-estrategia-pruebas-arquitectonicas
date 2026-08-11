using SistemaTareas.Application.Contracts;
using SistemaTareas.Domain.Entities;

namespace SistemaTareas.Application.UseCases.CrearTarea;

public sealed class CrearTareaUseCase(ITareaRepository tareas) : ICrearTareaUseCase
{
    public async Task<CrearTareaResult> ExecuteAsync(
        CrearTareaCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Titulo))
        {
            throw new ArgumentException("El título es obligatorio.", nameof(command));
        }

        if (command.Titulo.Length > 160)
        {
            throw new ArgumentException("El título no puede superar 160 caracteres.", nameof(command));
        }

        var descripcion = command.Descripcion ?? string.Empty;
        if (descripcion.Length > 800)
        {
            throw new ArgumentException("La descripción no puede superar 800 caracteres.", nameof(command));
        }

        var tarea = new Tarea(Guid.NewGuid(), command.Titulo, descripcion);
        await tareas.AgregarAsync(tarea, cancellationToken);
        await tareas.GuardarCambiosAsync(cancellationToken);

        return new(tarea.Id, "La tarea se creó correctamente y está lista para asignarse.");
    }
}
