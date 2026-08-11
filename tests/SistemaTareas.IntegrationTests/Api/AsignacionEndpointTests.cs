using Xunit;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SistemaTareas.Infrastructure.Seed;

namespace SistemaTareas.IntegrationTests.Api;

public sealed class AsignacionEndpointTests
{
    [Fact]
    public async Task Post_CuandoAsignacionEsValida_RetornaOk()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/tareas/{SeedData.RevisarPlanosId}/asignacion",
            new { usuarioId = SeedData.AnaId },
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_CuandoTareaNoExiste_RetornaNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/tareas/{Guid.NewGuid()}/asignacion",
            new { usuarioId = SeedData.AnaId },
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_CuandoTareaYaFueAsignada_RetornaConflict()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var url = $"/api/tareas/{SeedData.RevisarPlanosId}/asignacion";

        var first = await client.PostAsJsonAsync(
            url,
            new { usuarioId = SeedData.AnaId },
            cancellationToken);
        var second = await client.PostAsJsonAsync(
            url,
            new { usuarioId = SeedData.BrunoId },
            cancellationToken);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
