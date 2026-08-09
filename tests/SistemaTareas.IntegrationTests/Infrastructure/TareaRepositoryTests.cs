using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SistemaTareas.Application.Exceptions;
using SistemaTareas.Domain.Entities;
using SistemaTareas.Domain.Enums;
using SistemaTareas.Infrastructure.Repositories;
using SistemaTareas.Infrastructure.Seed;

namespace SistemaTareas.IntegrationTests.Infrastructure;

public sealed class TareaRepositoryTests
{
    [Fact]
    public async Task GuardarCambiosAsync_PersisteAsignacionEnSqlite()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();

        await using (var writeContext = database.CreateContext())
        {
            var tarea = await writeContext.Tareas.SingleAsync(
                x => x.Id == SeedData.RevisarPlanosId,
                cancellationToken);
            var usuario = await writeContext.Usuarios.SingleAsync(
                x => x.Id == SeedData.AnaId,
                cancellationToken);
            tarea.AsignarA(usuario, new DateTimeOffset(2026, 8, 8, 15, 0, 0, TimeSpan.Zero));
            await new TareaRepository(writeContext).GuardarCambiosAsync(cancellationToken);
        }

        await using var readContext = database.CreateContext();
        var persisted = await readContext.Tareas.AsNoTracking()
            .SingleAsync(x => x.Id == SeedData.RevisarPlanosId, cancellationToken);

        persisted.Estado.Should().Be(EstadoTarea.Asignada);
        persisted.UsuarioAsignadoId.Should().Be(SeedData.AnaId);
        persisted.AsignadaEn.Should().NotBeNull();
    }

    [Fact]
    public async Task GuardarCambiosAsync_CuandoUsuarioNoExiste_RespetaClaveForanea()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();
        await using var context = database.CreateContext();
        var tarea = await context.Tareas.SingleAsync(
            x => x.Id == SeedData.RevisarPlanosId,
            cancellationToken);
        tarea.AsignarA(new Usuario(Guid.NewGuid(), "Usuario externo"), DateTimeOffset.UtcNow);

        var action = () => context.SaveChangesAsync(cancellationToken);

        await action.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GuardarCambiosAsync_CuandoDosProcesosAsignanLaMismaTarea_DetectaConflicto()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var database = new SqliteTestDatabase();
        await database.InitializeAsync();
        await using var firstContext = database.CreateContext();
        await using var secondContext = database.CreateContext();
        var first = await firstContext.Tareas.SingleAsync(
            x => x.Id == SeedData.RevisarPlanosId,
            cancellationToken);
        var second = await secondContext.Tareas.SingleAsync(
            x => x.Id == SeedData.RevisarPlanosId,
            cancellationToken);
        var usuario = new Usuario(SeedData.AnaId, "Ana Rodríguez");

        first.AsignarA(usuario, DateTimeOffset.UtcNow);
        second.AsignarA(usuario, DateTimeOffset.UtcNow.AddSeconds(1));
        await new TareaRepository(firstContext).GuardarCambiosAsync(cancellationToken);

        var action = () => new TareaRepository(secondContext).GuardarCambiosAsync(cancellationToken);

        await action.Should().ThrowAsync<ConflictoConcurrenciaException>();
    }
}
