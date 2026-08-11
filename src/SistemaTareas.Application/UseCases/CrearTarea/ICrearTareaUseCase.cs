namespace SistemaTareas.Application.UseCases.CrearTarea;

public interface ICrearTareaUseCase
{
    Task<CrearTareaResult> ExecuteAsync(
        CrearTareaCommand command,
        CancellationToken cancellationToken);
}

