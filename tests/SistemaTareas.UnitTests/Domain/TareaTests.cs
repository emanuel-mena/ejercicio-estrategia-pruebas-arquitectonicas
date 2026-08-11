using Xunit;
using FluentAssertions;
using SistemaTareas.Domain.Entities;
using SistemaTareas.Domain.Enums;
using SistemaTareas.Domain.Exceptions;

namespace SistemaTareas.UnitTests.Domain;

public sealed class TareaTests
{
    private static readonly DateTimeOffset FechaAsignacion =
        new(2026, 8, 8, 14, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_CuandoIdEstaVacio_RechazaTarea()
    {
        var action = () => new Tarea(Guid.Empty, "Revisar planos");

        action.Should().Throw<ArgumentException>()
            .WithParameterName("id");
    }

    [Fact]
    public void Constructor_CuandoTituloEstaVacio_RechazaTarea()
    {
        var action = () => new Tarea(Guid.NewGuid(), "   ");

        action.Should().Throw<ArgumentException>()
            .WithParameterName("titulo");
    }

    [Fact]
    public void AsignarA_CuandoTareaEstaPendiente_AsignaUsuarioYFecha()
    {
        var tarea = NuevaTarea();
        var usuario = NuevoUsuario();

        tarea.AsignarA(usuario, FechaAsignacion);

        tarea.Estado.Should().Be(EstadoTarea.Asignada);
        tarea.UsuarioAsignadoId.Should().Be(usuario.Id);
        tarea.AsignadaEn.Should().Be(FechaAsignacion);
    }

    [Fact]
    public void AsignarA_CuandoUsuarioEstaInactivo_RechazaAsignacion()
    {
        var tarea = NuevaTarea();
        var usuario = new Usuario(Guid.NewGuid(), "Persona inactiva", false);

        var action = () => tarea.AsignarA(usuario, FechaAsignacion);

        action.Should().Throw<ReglaDominioException>()
            .Where(exception => exception.Codigo == "USUARIO_INACTIVO");
        tarea.Estado.Should().Be(EstadoTarea.Pendiente);
    }

    [Fact]
    public void AsignarA_CuandoTareaYaEstaAsignada_RechazaReasignacion()
    {
        var tarea = NuevaTarea();
        tarea.AsignarA(NuevoUsuario(), FechaAsignacion);

        var action = () => tarea.AsignarA(NuevoUsuario(), FechaAsignacion.AddMinutes(1));

        action.Should().Throw<ReglaDominioException>()
            .Where(exception => exception.Codigo == "TAREA_YA_ASIGNADA");
    }

    [Theory]
    [InlineData(EstadoTarea.Completada)]
    [InlineData(EstadoTarea.Cancelada)]
    public void AsignarA_CuandoEstadoNoEsPendiente_RechazaAsignacion(EstadoTarea estado)
    {
        var tarea = new Tarea(Guid.NewGuid(), "Tarea cerrada", estado: estado);

        var action = () => tarea.AsignarA(NuevoUsuario(), FechaAsignacion);

        action.Should().Throw<ReglaDominioException>()
            .Where(exception => exception.Codigo == "ESTADO_INVALIDO");
    }

    private static Tarea NuevaTarea() => new(Guid.NewGuid(), "Revisar planos");

    private static Usuario NuevoUsuario() => new(Guid.NewGuid(), "Ana Rodríguez");
}
