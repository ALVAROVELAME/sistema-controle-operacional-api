using System.ComponentModel.DataAnnotations;

namespace SistemaControleOperacionalApi.Extensions;

/// <summary>
/// Filtro de endpoint que valida DataAnnotations automaticamente.
/// Necessario porque Minimal APIs do .NET 8 nao validam DTOs sozinhos.
/// </summary>
public sealed class ValidationFilter<T> : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext ctx,
        EndpointFilterDelegate next)
    {
        var argumento = ctx.Arguments.OfType<T>().FirstOrDefault();
        if (argumento is not null)
        {
            var resultados = new List<ValidationResult>();
            var contexto = new ValidationContext(argumento);

            if (!Validator.TryValidateObject(
                    argumento,
                    contexto,
                    resultados,
                    validateAllProperties: true))
            {
                var erros = resultados
                    .Select(r => r.ErrorMessage ?? "Dado invalido")
                    .ToArray();

                return Results.BadRequest(new
                {
                    sucesso = false,
                    mensagem = erros.FirstOrDefault() ?? "Dados invalidos",
                    erros
                });
            }
        }

        return await next(ctx);
    }
}