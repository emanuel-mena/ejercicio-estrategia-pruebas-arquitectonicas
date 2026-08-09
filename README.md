# Sistema de Tareas — Grupo Barva

Proyecto académico funcional en C#/.NET 10 para demostrar el flujo crítico **Asignar tarea** con Razor Pages, DaisyUI, Entity Framework Core y SQLite.

## Alcance

La aplicación permite consultar tareas precargadas y asignar una tarea pendiente a un usuario activo. No implementa autenticación, creación o finalización de tareas, reportes, auditoría ni notificaciones.

## Requisitos

- .NET SDK 10.0.302 o un parche compatible de .NET 10.
- PowerShell para los scripts de Playwright y rendimiento.
- Acceso a Internet al abrir la interfaz, porque DaisyUI y Tailwind se sirven desde CDN.

## Ejecutar la aplicación

```powershell
dotnet restore SistemaTareas.slnx
dotnet run --project src/SistemaTareas.Web
```

Abra `http://localhost:5080`. En el primer inicio se crea `src/SistemaTareas.Web/App_Data/tareas.db` y se insertan tres tareas y cuatro usuarios de demostración.

## API

```http
GET /api/tareas/pendientes

POST /api/tareas/{tareaId}/asignacion
Content-Type: application/json

{
  "usuarioId": "10000000-0000-0000-0000-000000000001"
}
```

La API devuelve `200` para una asignación exitosa, `404` cuando no existe la tarea o el usuario y `409` cuando una regla o la concurrencia impide la operación.

## Pruebas

### Unitarias, integración y arquitectura

```powershell
dotnet test tests/SistemaTareas.UnitTests
dotnet test tests/SistemaTareas.IntegrationTests
dotnet test tests/SistemaTareas.ArchitectureTests
```

Las unitarias usan mocks únicamente en `ITareaRepository` e `IUsuarioRepository`. Las pruebas de persistencia utilizan el proveedor SQLite real en memoria; no simulan `DbSet`.

### Sistema con Chromium

Instale el navegador una vez y ejecute el proyecto E2E:

```powershell
dotnet build tests/SistemaTareas.SystemTests
& "tests/SistemaTareas.SystemTests/bin/Debug/net10.0/playwright.ps1" install chromium
dotnet test tests/SistemaTareas.SystemTests --no-build
```

La prueba inicia una instancia aislada de la aplicación, utiliza una base temporal, completa la asignación desde Chromium y cierra todos los recursos.

### Cobertura

```powershell
dotnet test tests/SistemaTareas.UnitTests --collect:"XPlat Code Coverage" --results-directory artifacts/coverage
```

La meta documentada es al menos 90 % para las clases centrales `Tarea` y `AsignarTareaUseCase`.

### Rendimiento

El script inicia una aplicación aislada con suficientes tareas, ejecuta NBomber y la cierra automáticamente:

```powershell
& ./scripts/run-performance-test.ps1
```

Para una verificación corta:

```powershell
& ./scripts/run-performance-test.ps1 -DurationSeconds 5 -ExtraPendingTasks 2000
```

Umbrales: 10 usuarios concurrentes, p95 ≤ 500 ms, p99 ≤ 1 segundo y errores ≤ 1 %. Los reportes HTML se generan en `artifacts/performance`. NBomber 6 informa que su edición gratuita es para uso personal; este proyecto la usa únicamente en el contexto académico indicado.

## Estructura y entregables

- `src/SistemaTareas.Domain`: entidades y reglas puras.
- `src/SistemaTareas.Application`: caso de uso, consultas y puertos.
- `src/SistemaTareas.Infrastructure`: EF Core, SQLite, repositorios y datos semilla.
- `src/SistemaTareas.Web`: Razor Pages, DaisyUI y API.
- `tests`: pruebas unitarias, integración, arquitectura, sistema y rendimiento.
- [Arquitectura y diagramas](docs/arquitectura.md).
- [Matriz requisito-prueba](docs/matriz-requisito-prueba.md).
- [ADR de arquitectura](docs/ADR-001-monolito-modular-en-capas.md).

## Decisiones relevantes

- Repository protege al dominio de EF Core y SQLite.
- `TimeProvider` hace deterministas las fechas en pruebas.
- El `Guid Version` de `Tarea` es un token de concurrencia optimista compatible con SQLite.
- La interfaz y la API llaman al mismo `AsignarTareaUseCase`.
- La UI no contiene menús, reportes ni detalles sobre la ejecución de pruebas.

