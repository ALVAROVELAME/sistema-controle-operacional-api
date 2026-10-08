using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using SistemaControleOperacionalApi.DTOs.Comum;

namespace SistemaControleOperacionalApi.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, mensagem) = MapException(exception);

        if (status >= 500)
            _logger.LogError(exception, "Erro nao tratado: {Message}", exception.Message);
        else
            _logger.LogWarning("Erro tratado [{Status}]: {Message}", status, mensagem);

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        await httpContext.Response.WriteAsJsonAsync(
            new MensagemResponseDTO(mensagem, sucesso: false),
            cancellationToken);

        return true;
    }

    private static (int Status, string Mensagem) MapException(Exception ex) => ex switch
    {
        AppException app              => (app.StatusCode, app.Message),
        ValidationException v         => (StatusCodes.Status400BadRequest, v.Message),
        ArgumentException a           => (StatusCodes.Status400BadRequest, a.Message),
        UnauthorizedAccessException u => (StatusCodes.Status401Unauthorized, u.Message),
        KeyNotFoundException k        => (StatusCodes.Status404NotFound, k.Message),
        _                             => (StatusCodes.Status500InternalServerError, "Erro interno do servidor.")
    };
}