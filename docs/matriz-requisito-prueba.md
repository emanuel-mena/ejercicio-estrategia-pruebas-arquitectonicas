# Matriz requisito — prueba arquitectónica

| ID | Requisito del flujo Barva | Tipo | Herramienta | Evidencia automatizada | Umbral esperado |
|---|---|---|---|---|---|
| BARVA-01 | Asignar una tarea pendiente a un usuario activo | Unidad | xUnit v3 + FluentAssertions | `TareaTests` | Estado `Asignada`, usuario y fecha correctos |
| BARVA-02 | Rechazar usuarios inactivos | Unidad | xUnit v3 | `TareaTests` y `AsignarTareaUseCaseTests` | Sin escritura en repositorio |
| BARVA-03 | Rechazar tareas ya asignadas, completadas o canceladas | Unidad | xUnit v3 | `TareaTests` | Excepción/regla de dominio esperada |
| BARVA-04 | No persistir cuando falta la tarea o el usuario | Unidad | Moq | `AsignarTareaUseCaseTests` | `GuardarCambiosAsync` se invoca 0 veces |
| BARVA-05 | Persistir la asignación en una base relacional real | Integración | EF Core + SQLite en memoria | `TareaRepositoryTests` | Datos recuperables desde un contexto nuevo |
| BARVA-06 | Mantener integridad referencial | Integración | SQLite | `GuardarCambiosAsync_CuandoUsuarioNoExiste_RespetaClaveForanea` | SQLite rechaza la FK inválida |
| BARVA-07 | Evitar pérdida de actualización concurrente | Integración | EF Core concurrency token | `GuardarCambiosAsync_CuandoDosProcesosAsignanLaMismaTarea_DetectaConflicto` | Segundo guardado produce conflicto |
| BARVA-08 | Exponer resultados HTTP consistentes | Integración | WebApplicationFactory | `AsignacionEndpointTests` | 200 éxito, 404 inexistente, 409 conflicto |
| BARVA-09 | Completar el flujo desde un navegador real | Sistema | Playwright + Chromium | `AsignarTareaSystemTests` | La asignación continúa visible tras recargar |
| ARCH-01 | Domain no depende de capas externas | Arquitectura | NetArchTest | `LayerDependencyTests` | 0 dependencias prohibidas |
| ARCH-02 | Application no depende de Web o Infrastructure | Arquitectura | NetArchTest | `LayerDependencyTests` | 0 dependencias prohibidas |
| ARCH-03 | PageModels no acceden a persistencia | Arquitectura | NetArchTest | `LayerDependencyTests` | 0 tipos infractores |
| PERF-01 | Asignar bajo carga concurrente moderada | Rendimiento | NBomber | Escenario `asignar_tarea` | 10 usuarios, p95 ≤ 500 ms, p99 ≤ 1 s, errores ≤ 1 % |
| QUAL-01 | Mantener cobertura alta en el núcleo Barva | Calidad | Coverlet | Reporte Cobertura XML | Meta ≥ 90 % para `Tarea` y `AsignarTareaUseCase` |

La cobertura es una señal complementaria. La aceptación principal se basa en comportamientos y reglas explícitas, no en alcanzar un porcentaje aislado.

