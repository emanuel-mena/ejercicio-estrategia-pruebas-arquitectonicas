namespace SistemaTareas.Application.UseCases.AsignarTarea;

public interface IAsignarTareaUseCase
{
    Task<AsignarTareaResult> ExecuteAsync(
        AsignarTareaCommand command,
        CancellationToken cancellationToken);
}

