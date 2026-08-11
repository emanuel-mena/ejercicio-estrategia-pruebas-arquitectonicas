using Microsoft.Extensions.DependencyInjection;
using SistemaTareas.Application.Queries;
using SistemaTareas.Application.UseCases.AsignarTarea;
using SistemaTareas.Application.UseCases.CrearTarea;

namespace SistemaTareas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAsignarTareaUseCase, AsignarTareaUseCase>();
        services.AddScoped<ICrearTareaUseCase, CrearTareaUseCase>();
        services.AddScoped<IConsultaTareas, ConsultaTareas>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
