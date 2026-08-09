namespace SistemaTareas.Application.Exceptions;

public sealed class ConflictoConcurrenciaException(string message, Exception? innerException = null)
    : Exception(message, innerException);

