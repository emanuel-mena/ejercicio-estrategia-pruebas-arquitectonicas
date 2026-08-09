using SistemaTareas.Domain.Enums;
using SistemaTareas.Domain.Exceptions;

namespace SistemaTareas.Domain.Entities;

public sealed class Tarea
{
    private Tarea()
    {
    }

    public Tarea(
        Guid id,
        string titulo,
        string descripcion = "",
        EstadoTarea estado = EstadoTarea.Pendiente)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador de la tarea es obligatorio.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new ArgumentException("El título de la tarea es obligatorio.", nameof(titulo));
        }

        Id = id;
        Titulo = titulo.Trim();
        Descripcion = descripcion.Trim();
        Estado = estado;
        Version = Guid.NewGuid();
    }

    public Guid Id { get; private set; }

    public string Titulo { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public EstadoTarea Estado { get; private set; }

    public Guid? UsuarioAsignadoId { get; private set; }

    public DateTimeOffset? AsignadaEn { get; private set; }

    public Guid Version { get; private set; }

    public void AsignarA(Usuario usuario, DateTimeOffset fecha)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        if (!usuario.Activo)
        {
            throw new ReglaDominioException("USUARIO_INACTIVO", "El usuario seleccionado está inactivo.");
        }

        if (Estado == EstadoTarea.Asignada || UsuarioAsignadoId.HasValue)
        {
            throw new ReglaDominioException("TAREA_YA_ASIGNADA", "La tarea ya fue asignada.");
        }

        if (Estado != EstadoTarea.Pendiente)
        {
            throw new ReglaDominioException("ESTADO_INVALIDO", "Solo se pueden asignar tareas pendientes.");
        }

        UsuarioAsignadoId = usuario.Id;
        AsignadaEn = fecha;
        Estado = EstadoTarea.Asignada;
        Version = Guid.NewGuid();
    }
}

