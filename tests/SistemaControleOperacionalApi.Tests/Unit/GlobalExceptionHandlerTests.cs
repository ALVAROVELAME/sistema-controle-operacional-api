using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SistemaControleOperacionalApi.Exceptions;

namespace SistemaControleOperacionalApi.Tests.Unit;

public sealed class GlobalExceptionHandlerTests
{
    public static TheoryData<Exception, int, string> Cases => new()
    {
        { new ConflictException("Conflito"), 409, "Conflito" },
        { new ForbiddenException("Proibido"), 403, "Proibido" },
        { new NotFoundException("Ausente"), 404, "Ausente" },
        { new ValidationException("Invalido"), 400, "Invalido" },
        { new ArgumentException("Argumento"), 400, "Argumento" },
        { new UnauthorizedAccessException("Negado"), 401, "Negado" },
        { new KeyNotFoundException("Chave"), 404, "Chave" },
        { new Exception("segredo interno"), 500, "Erro interno do servidor." }
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Mapeia_status_e_nao_expoe_detalhes_internos(Exception exception, int status, string message)
    {
        var context = new DefaultHttpContext();
        using var stream = new MemoryStream();
        context.Response.Body = stream;
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        (await handler.TryHandleAsync(context, exception, CancellationToken.None)).Should().BeTrue();
        context.Response.StatusCode.Should().Be(status);
        stream.Position = 0;
        var json = await JsonSerializer.DeserializeAsync<JsonElement>(stream);
        json.GetProperty("sucesso").GetBoolean().Should().BeFalse();
        json.GetProperty("mensagem").GetString().Should().Be(message);
    }
}
