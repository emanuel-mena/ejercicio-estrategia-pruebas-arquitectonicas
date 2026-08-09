using SistemaTareas.Application.Queries;
using SistemaTareas.Application.UseCases.AsignarTarea;

namespace SistemaTareas.Web.Api;

public static class AsignacionEndpoints
{
    public static IEndpointRouteBuilder MapAsignacionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/tareas/pendientes", async (
            IConsultaTareas consulta,
            CancellationToken cancellationToken) =>
        {
            var tareas = await consulta.ListarTareasAsync(cancellationToken);
            return Results.Ok(tareas.Where(tarea => tarea.Estado == "Pendiente"));
        });

        endpoints.MapPost("/api/tareas/{tareaId:guid}/asignacion", async (
            Guid tareaId,
            AsignacionRequest request,
            IAsignarTareaUseCase useCase,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.ExecuteAsync(
                new AsignarTareaCommand(tareaId, request.UsuarioId),
                cancellationToken);

            return result.Codigo switch
            {
                CodigoAsignacion.Exito => Results.Ok(result),
                CodigoAsignacion.TareaNoEncontrada or CodigoAsignacion.UsuarioNoEncontrado =>
                    Results.NotFound(result),
                _ => Results.Conflict(result)
            };
        });

        return endpoints;
    }

    public sealed record AsignacionRequest(Guid UsuarioId);
}

