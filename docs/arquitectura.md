# Arquitectura del Sistema de Tareas — Grupo Barva

## Objetivo

El sistema implementa el flujo crítico **Asignar tarea** y una función auxiliar para crear tareas durante la demostración. Completar, notificar, auditar y reportar quedan fuera del alcance funcional y de la estrategia de pruebas del grupo Barva.

La solución es un monolito modular en capas. Se despliega como una sola aplicación, pero conserva fronteras explícitas para que el dominio y la aplicación no dependan de la web, Entity Framework Core ni SQLite.

## C4 nivel 1 — Contexto

```mermaid
C4Context
    title Sistema de Tareas - Contexto
    Person(coordinador, "Coordinador", "Asigna tareas a integrantes activos")
    System(sistema, "Sistema de Tareas", "Permite crear, consultar y asignar tareas")
    Rel(coordinador, sistema, "Crea, consulta y asigna tareas", "HTTP/HTML")
```

## C4 nivel 2 — Contenedores

```mermaid
C4Container
    title Sistema de Tareas - Contenedores
    Person(coordinador, "Coordinador", "Usuario de demostración")
    Container(web, "Aplicación web", "ASP.NET Core 10 / Razor Pages", "Interfaz web y API HTTP")
    ContainerDb(db, "Base de datos", "SQLite", "Usuarios, tareas y asignaciones")
    Rel(coordinador, web, "Usa", "HTTP")
    Rel(web, db, "Lee y persiste", "EF Core / SQL")
```

## C4 nivel 3 — Componentes

```mermaid
C4Component
    title Sistema de Tareas - Componentes
    Container_Boundary(web, "ASP.NET Core") {
        Component(pages, "Razor Pages", "Presentación", "Crea y lista tareas; recibe la asignación")
        Component(api, "API de asignación", "Minimal API", "Expone el caso de uso por HTTP")
        Component(usecase, "AsignarTareaUseCase", "Aplicación", "Orquesta el flujo y traduce resultados")
        Component(createcase, "CrearTareaUseCase", "Aplicación", "Crea tareas pendientes para demostración")
        Component(domain, "Tarea", "Dominio", "Protege las reglas de asignación")
        Component(ports, "Contratos de repositorio", "Aplicación", "Puertos de persistencia")
        Component(adapters, "Repositorios EF Core", "Infraestructura", "Implementa los puertos")
    }
    ContainerDb(db, "SQLite", "Base de datos", "Persistencia local")
    Rel(pages, usecase, "Ejecuta")
    Rel(pages, createcase, "Ejecuta")
    Rel(api, usecase, "Ejecuta")
    Rel(usecase, domain, "Invoca")
    Rel(createcase, domain, "Crea")
    Rel(usecase, ports, "Depende de")
    Rel(createcase, ports, "Depende de")
    Rel(adapters, ports, "Implementa")
    Rel(adapters, db, "Lee/escribe")
```

La única excepción de composición es `SistemaTareas.Web`: referencia Infrastructure para registrar adaptadores en `Program.cs`. Los PageModels no conocen `DbContext` ni repositorios.

## Secuencia del flujo Barva

```mermaid
sequenceDiagram
    actor C as Coordinador
    participant W as Razor Page / API
    participant A as AsignarTareaUseCase
    participant TR as ITareaRepository
    participant UR as IUsuarioRepository
    participant D as Tarea
    participant DB as SQLite

    C->>W: TareaId + UsuarioId
    W->>A: ExecuteAsync(command)
    A->>TR: ObtenerPorIdAsync
    TR->>DB: SELECT Tarea
    DB-->>TR: Tarea o null
    A->>UR: ObtenerPorIdAsync
    UR->>DB: SELECT Usuario
    DB-->>UR: Usuario o null
    A->>D: AsignarA(usuario, ahora)
    D->>D: Validar estado y usuario activo
    A->>TR: GuardarCambiosAsync
    TR->>DB: UPDATE con token Version
    DB-->>TR: Confirmación o conflicto
    A-->>W: Resultado tipado
    W-->>C: Confirmación o mensaje de error
```

## Reglas protegidas

- Domain no conoce Application, Infrastructure, Web ni Entity Framework.
- Application define casos de uso y contratos; no conoce adaptadores.
- Infrastructure implementa contratos con EF Core y SQLite.
- Web consume casos de uso, nunca `DbContext` ni repositorios directamente.
- La columna `Version` es un token de concurrencia optimista; dos asignaciones simultáneas no pueden sobrescribirse silenciosamente.
