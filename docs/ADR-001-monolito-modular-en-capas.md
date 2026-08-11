# ADR-001: monolito modular en capas

- Estado: aceptada
- Fecha: 2026-08-08
- Decisores: grupo Barva

## Contexto

El ejercicio requiere un sistema pequeño, funcional y demostrable para el flujo “Asignar tarea”, una base SQLite y pruebas arquitectónicas reales. La creación de tareas se incorpora como soporte de demostración para no depender únicamente de los tres registros semilla. El equipo es pequeño, el dominio es acotado y no existen requisitos de despliegue independiente o escala distribuida.

## Decisión

Implementar un monolito modular con cuatro proyectos: Domain, Application, Infrastructure y Web. Las dependencias apuntan hacia el dominio; Repository y Dependency Injection aíslan la persistencia. Razor Pages y una Minimal API reutilizan el mismo caso de uso.

## Alternativas consideradas

| Alternativa | Ventaja | Motivo de descarte |
|---|---|---|
| Aplicación CRUD en un solo proyecto | Menos archivos | Hace difícil demostrar y automatizar fronteras arquitectónicas |
| SPA y API separadas | Separación de frontend | Duplica configuración y despliegue para una interfaz mínima |
| Microservicios | Despliegue independiente | Coste operativo injustificado para un único flujo y una base local |

## Consecuencias

Positivas:

- Reglas de negocio aisladas y fáciles de probar.
- SQLite puede sustituirse sin modificar Domain o Application.
- Las reglas entre capas se validan automáticamente.
- Un único proceso simplifica la demostración.

Negativas:

- Existen más proyectos y contratos que en un CRUD directo.
- Web referencia Infrastructure en el composition root.
- SQLite limita la escritura concurrente; se mitiga con operaciones cortas y detección de conflictos.

## Riesgo aceptado

El sistema no implementa autenticación ni los flujos de otros grupos. Los datos semilla permiten demostrar Barva sin expandir el alcance.
