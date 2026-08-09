using Microsoft.Extensions.DependencyInjection;
using SistemaTareas.Application.Queries;
using SistemaTareas.Application.UseCases.AsignarTarea;

namespace SistemaTareas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAsignarTareaUseCase, AsignarTareaUseCase>();
        services.AddScoped<IConsultaTareas, ConsultaTareas>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
