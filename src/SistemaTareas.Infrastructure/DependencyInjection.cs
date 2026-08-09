using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaTareas.Application.Contracts;
using SistemaTareas.Infrastructure.Persistence;
using SistemaTareas.Infrastructure.Repositories;

namespace SistemaTareas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<TareasDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ITareaRepository, TareaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        return services;
    }
}
