namespace SistemaTareas.Application.UseCases.AsignarTarea;

public sealed record AsignarTareaCommand(Guid TareaId, Guid UsuarioId);

