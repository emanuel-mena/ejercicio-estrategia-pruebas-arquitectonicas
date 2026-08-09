namespace SistemaTareas.Application.UseCases.AsignarTarea;

public enum CodigoAsignacion
{
    Exito,
    TareaNoEncontrada,
    UsuarioNoEncontrado,
    UsuarioInactivo,
    TareaYaAsignada,
    EstadoInvalido,
    ConflictoConcurrencia
}

public sealed record AsignarTareaResult(CodigoAsignacion Codigo, string Mensaje)
{
    public bool EsExitoso => Codigo == CodigoAsignacion.Exito;
}

