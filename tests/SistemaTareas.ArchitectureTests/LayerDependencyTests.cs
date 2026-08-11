using Xunit;
using FluentAssertions;
using NetArchTest.Rules;
using SistemaTareas.Application.Contracts;
using SistemaTareas.Domain.Entities;
using SistemaTareas.Infrastructure.Persistence;
using SistemaTareas.Infrastructure.Repositories;
using SistemaTareas.Web.Pages;

namespace SistemaTareas.ArchitectureTests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_NoDebeDependerDeCapasExternas()
    {
        var result = Types.InAssembly(typeof(Tarea).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SistemaTareas.Application",
                "SistemaTareas.Infrastructure",
                "SistemaTareas.Web",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(DescribeFailures(result));
    }

    [Fact]
    public void Application_NoDebeDependerDeInfrastructureNiWeb()
    {
        var result = Types.InAssembly(typeof(ITareaRepository).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("SistemaTareas.Infrastructure", "SistemaTareas.Web")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(DescribeFailures(result));
    }

    [Fact]
    public void PageModels_NoDebenAccederADbContextNiRepositorios()
    {
        var result = Types.InAssembly(typeof(IndexModel).Assembly)
            .That()
            .ResideInNamespace("SistemaTareas.Web.Pages")
            .ShouldNot()
            .HaveDependencyOnAny(
                typeof(TareasDbContext).Namespace!,
                typeof(TareaRepository).Namespace!,
                typeof(ITareaRepository).Namespace!)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(DescribeFailures(result));
    }

    [Fact]
    public void Repositorios_DebenImplementarContratosDeAplicacion()
    {
        typeof(ITareaRepository).IsAssignableFrom(typeof(TareaRepository)).Should().BeTrue();
        typeof(IUsuarioRepository).IsAssignableFrom(typeof(UsuarioRepository)).Should().BeTrue();
    }

    private static string DescribeFailures(NetArchTest.Rules.TestResult result) =>
        result.FailingTypeNames is null
            ? "No se reportaron tipos infractores."
            : $"Tipos infractores: {string.Join(", ", result.FailingTypeNames)}";
}
