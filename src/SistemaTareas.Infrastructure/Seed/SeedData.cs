using SistemaTareas.Domain.Entities;

namespace SistemaTareas.Infrastructure.Seed;

public static class SeedData
{
    public static readonly Guid AnaId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid BrunoId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid CarlaId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid UsuarioInactivoId = Guid.Parse("10000000-0000-0000-0000-000000000099");

    public static readonly Guid RevisarPlanosId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid CoordinarInspeccionId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid ActualizarCronogramaId = Guid.Parse("20000000-0000-0000-0000-000000000003");

    public static IReadOnlyList<Usuario> Usuarios =>
    [
        new(AnaId, "Ana Rodríguez"),
        new(BrunoId, "Bruno Vargas"),
        new(CarlaId, "Carla Méndez"),
        new(UsuarioInactivoId, "Usuario inactivo", false)
    ];

    public static IReadOnlyList<Tarea> Tareas =>
    [
        new(RevisarPlanosId, "Revisar planos estructurales", "Validar la última versión de los planos del edificio norte."),
        new(CoordinarInspeccionId, "Coordinar inspección de obra", "Confirmar fecha y responsable de la inspección semanal."),
        new(ActualizarCronogramaId, "Actualizar cronograma", "Incorporar los avances reportados por los equipos de campo.")
    ];
}

