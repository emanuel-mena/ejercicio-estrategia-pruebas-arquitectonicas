using FluentAssertions;
using Moq;
using SistemaTareas.Application.Contracts;
using SistemaTareas.Application.Exceptions;
using SistemaTareas.Application.UseCases.AsignarTarea;
using SistemaTareas.Domain.Entities;

namespace SistemaTareas.UnitTests.Application;

public sealed class AsignarTareaUseCaseTests
{
    private static readonly Guid TareaId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid UsuarioId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly DateTimeOffset Ahora = new(2026, 8, 8, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecuteAsync_CuandoDatosSonValidos_GuardaUnaVez()
    {
        var tarea = new Tarea(TareaId, "Revisar planos");
        var usuario = new Usuario(UsuarioId, "Ana Rodríguez");
        var tareas = CrearRepositorioTareas(tarea);
        var usuarios = CrearRepositorioUsuarios(usuario);
        var sut = CrearSut(tareas.Object, usuarios.Object);

        var result = await sut.ExecuteAsync(new(TareaId, UsuarioId), CancellationToken.None);

        result.Codigo.Should().Be(CodigoAsignacion.Exito);
        tarea.UsuarioAsignadoId.Should().Be(UsuarioId);
        tarea.AsignadaEn.Should().Be(Ahora);
        tareas.Verify(repository => repository.GuardarCambiosAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CuandoTareaNoExiste_NoConsultaUsuarioNiGuarda()
    {
        var tareas = CrearRepositorioTareas(null);
        var usuarios = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        var sut = CrearSut(tareas.Object, usuarios.Object);

        var result = await sut.ExecuteAsync(new(TareaId, UsuarioId), CancellationToken.None);

        result.Codigo.Should().Be(CodigoAsignacion.TareaNoEncontrada);
        usuarios.VerifyNoOtherCalls();
        tareas.Verify(
            repository => repository.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_CuandoUsuarioNoExiste_NoGuarda()
    {
        var tareas = CrearRepositorioTareas(new Tarea(TareaId, "Revisar planos"));
        var usuarios = CrearRepositorioUsuarios(null);
        var sut = CrearSut(tareas.Object, usuarios.Object);

        var result = await sut.ExecuteAsync(new(TareaId, UsuarioId), CancellationToken.None);

        result.Codigo.Should().Be(CodigoAsignacion.UsuarioNoEncontrado);
        tareas.Verify(
            repository => repository.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_CuandoUsuarioEstaInactivo_NoGuarda()
    {
        var tareas = CrearRepositorioTareas(new Tarea(TareaId, "Revisar planos"));
        var usuarios = CrearRepositorioUsuarios(new Usuario(UsuarioId, "Usuario inactivo", false));
        var sut = CrearSut(tareas.Object, usuarios.Object);

        var result = await sut.ExecuteAsync(new(TareaId, UsuarioId), CancellationToken.None);

        result.Codigo.Should().Be(CodigoAsignacion.UsuarioInactivo);
        tareas.Verify(
            repository => repository.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_CuandoPersistenciaDetectaConcurrencia_RetornaConflicto()
    {
        var tareas = CrearRepositorioTareas(new Tarea(TareaId, "Revisar planos"));
        tareas
            .Setup(repository => repository.GuardarCambiosAsync(CancellationToken.None))
            .ThrowsAsync(new ConflictoConcurrenciaException("Conflicto"));
        var usuarios = CrearRepositorioUsuarios(new Usuario(UsuarioId, "Ana Rodríguez"));
        var sut = CrearSut(tareas.Object, usuarios.Object);

        var result = await sut.ExecuteAsync(new(TareaId, UsuarioId), CancellationToken.None);

        result.Codigo.Should().Be(CodigoAsignacion.ConflictoConcurrencia);
    }

    private static AsignarTareaUseCase CrearSut(
        ITareaRepository tareas,
        IUsuarioRepository usuarios) =>
        new(tareas, usuarios, new FixedTimeProvider(Ahora));

    private static Mock<ITareaRepository> CrearRepositorioTareas(Tarea? tarea)
    {
        var mock = new Mock<ITareaRepository>(MockBehavior.Strict);
        mock.Setup(repository => repository.ObtenerPorIdAsync(TareaId, CancellationToken.None))
            .ReturnsAsync(tarea);
        if (tarea is not null)
        {
            mock.Setup(repository => repository.GuardarCambiosAsync(CancellationToken.None))
                .Returns(Task.CompletedTask);
        }

        return mock;
    }

    private static Mock<IUsuarioRepository> CrearRepositorioUsuarios(Usuario? usuario)
    {
        var mock = new Mock<IUsuarioRepository>(MockBehavior.Strict);
        mock.Setup(repository => repository.ObtenerPorIdAsync(UsuarioId, CancellationToken.None))
            .ReturnsAsync(usuario);
        return mock;
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}

