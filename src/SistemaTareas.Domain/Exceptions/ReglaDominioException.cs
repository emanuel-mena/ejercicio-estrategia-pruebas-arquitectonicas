namespace SistemaTareas.Domain.Exceptions;

public sealed class ReglaDominioException(string codigo, string message) : InvalidOperationException(message)
{
    public string Codigo { get; } = codigo;
}

