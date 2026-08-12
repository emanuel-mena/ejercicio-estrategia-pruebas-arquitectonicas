# Matriz resumida de pruebas arquitectónicas — Flujo Barva

Esta versión para presentación resume una prueba automatizada por cada nivel solicitado para el flujo crítico **Asignar tarea**.

| ID | Tipo | Requisito validado | Herramienta | Evidencia automatizada | Métrica o resultado esperado |
|---|---|---|---|---|---|
| `BARVA-04` | Unidad | Si la tarea no existe, el caso de uso debe detenerse sin consultar al usuario ni persistir cambios | xUnit v3 + Moq + FluentAssertions | `ExecuteAsync_CuandoTareaNoExiste_NoConsultaUsuarioNiGuarda` | Resultado `TareaNoEncontrada`; 0 consultas al repositorio de usuarios y 0 guardados |
| `BARVA-05` | Integración | Una asignación debe persistirse y recuperarse desde una base de datos relacional real | EF Core + SQLite en memoria + xUnit v3 | `GuardarCambiosAsync_PersisteAsignacionEnSqlite` | Estado `Asignada`, usuario y fecha recuperables desde un contexto nuevo |
| `BARVA-09` | Sistema | El usuario debe poder asignar una tarea desde la interfaz y observar el cambio después de recargar | Playwright + Chromium + xUnit v3 | `UsuarioAsignaTareaDesdeLaInterfazYElCambioPersiste` | Mensaje de éxito y asignación a “Ana Rodríguez” visible después de recargar |
| `PERF-01` | Rendimiento | La API debe sostener asignaciones bajo carga concurrente moderada | NBomber | Escenario `asignar_tarea` | 10 usuarios concurrentes; p95 ≤ 500 ms; p99 ≤ 1 s; errores ≤ 1 % |

## 1. Prueba de unidad — BARVA-04

**Objetivo arquitectónico:** comprobar que el caso de uso respeta los puertos de persistencia y corta el flujo sin efectos secundarios cuando no encuentra la tarea.

**Archivo:** `tests/SistemaTareas.UnitTests/Application/AsignarTareaUseCaseTests.cs`

```csharp
var tareas = CrearRepositorioTareas(null);
var usuarios = new Mock<IUsuarioRepository>(MockBehavior.Strict);
var sut = CrearSut(tareas.Object, usuarios.Object);

var result = await sut.ExecuteAsync(new(TareaId, UsuarioId), CancellationToken.None);

result.Codigo.Should().Be(CodigoAsignacion.TareaNoEncontrada);
usuarios.VerifyNoOtherCalls();
tareas.Verify(
    repository => repository.GuardarCambiosAsync(It.IsAny<CancellationToken>()),
    Times.Never);
```

El mock estricto permite verificar el límite entre la capa de aplicación y sus repositorios: ante una tarea inexistente, no se consulta el usuario y `GuardarCambiosAsync` nunca se ejecuta.

## 2. Prueba de integración — BARVA-05

**Objetivo arquitectónico:** validar en conjunto la entidad de dominio, el repositorio EF Core y SQLite, incluyendo una lectura posterior desde otro contexto.

**Archivo:** `tests/SistemaTareas.IntegrationTests/Infrastructure/TareaRepositoryTests.cs`

```csharp
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
```

La escritura y la consulta usan contextos diferentes, por lo que las aserciones demuestran persistencia real en SQLite y no solamente el estado en memoria de una entidad.

## 3. Prueba de sistema — BARVA-09

**Objetivo arquitectónico:** recorrer el sistema desplegado desde la interfaz web hasta la persistencia mediante un navegador real.

**Archivo:** `tests/SistemaTareas.SystemTests/AsignarTareaSystemTests.cs`

```csharp
await page.GotoAsync(application.BaseUrl);
await page.GetByTestId("assign-link").First.ClickAsync();
await page.GetByTestId("assignee-select").SelectOptionAsync(new SelectOptionValue { Index = 1 });
await page.GetByTestId("assign-submit").ClickAsync();

await Assertions.Expect(page.GetByTestId("success-alert"))
    .ToContainTextAsync("se asignó correctamente");

await page.ReloadAsync();
await Assertions.Expect(page.GetByTestId("assigned-message"))
    .ToContainTextAsync("Ana Rodríguez");
```

Playwright reproduce la interacción del usuario. La comprobación posterior a `ReloadAsync` evidencia que el cambio atravesó la interfaz, el caso de uso y la base de datos.

## 4. Prueba de rendimiento — PERF-01

**Objetivo arquitectónico:** verificar que el endpoint de asignación cumple los límites de latencia y error bajo concurrencia sostenida.

**Archivo:** `tests/SistemaTareas.PerformanceTests/Program.cs`

```csharp
var scenario = Scenario.Create("asignar_tarea", async _ =>
    {
        if (!taskIds.TryDequeue(out var taskId))
        {
            return Response.Fail("NO_TASKS", "No quedan tareas pendientes para asignar.", 0, 0);
        }

        using var response = await client.PostAsJsonAsync(
            $"/api/tareas/{taskId}/asignacion",
            new { usuarioId = Guid.Parse("10000000-0000-0000-0000-000000000001") });
        await Task.Delay(100);

        return response.IsSuccessStatusCode
            ? Response.Ok()
            : Response.Fail(statusCode: response.StatusCode.ToString());
    })
    .WithoutWarmUp()
    .WithLoadSimulations(
        Simulation.KeepConstant(copies: 10, during: TimeSpan.FromSeconds(durationSeconds)))
    .WithThresholds(
        Threshold.Create(scenarioStats => scenarioStats.Ok.Latency.Percent95 <= 500),
        Threshold.Create(scenarioStats => scenarioStats.Ok.Latency.Percent99 <= 1000),
        Threshold.Create(scenarioStats => scenarioStats.Fail.Request.Percent <= 1));
```

Cada usuario virtual toma una tarea distinta y llama al endpoint real. NBomber mantiene 10 copias concurrentes y hace fallar la ejecución si se supera cualquiera de los tres umbrales.
