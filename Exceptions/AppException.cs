namespace SistemaControleOperacionalApi.Exceptions;

/// <summary>
/// Excecao base para erros de negocio previsiveis.
/// Equivalente a lancar RuntimeException no Java (tratada como 400).
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, 403) { }
}