namespace SistemaControleOperacionalApi.Services;

public interface IEmailService
{
    Task EnviarEmailRecuperacaoAsync(string email, string nome, string token);
    Task EnviarEmailConfirmacaoAsync(string email, string nome, string token);
}
