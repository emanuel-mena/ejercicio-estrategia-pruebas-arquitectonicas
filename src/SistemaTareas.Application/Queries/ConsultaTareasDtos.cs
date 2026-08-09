namespace SistemaTareas.Application.Queries;

public sealed record TareaResumen(
    Guid Id,
    string Titulo,
    string Descripcion,
    string Estado,
    Guid? UsuarioAsignadoId,
    string? UsuarioAsignado,
    DateTimeOffset? AsignadaEn);

public sealed record UsuarioResumen(Guid Id, string Nombre);

