namespace SistemaTareas.Domain.Entities;

public sealed class Usuario
{
    private Usuario()
    {
    }

    public Usuario(Guid id, string nombre, bool activo = true)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador del usuario es obligatorio.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del usuario es obligatorio.", nameof(nombre));
        }

        Id = id;
        Nombre = nombre.Trim();
        Activo = activo;
    }

    public Guid Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public bool Activo { get; private set; }
}

