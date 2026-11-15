namespace SistemaControleOperacionalApi.Services;

public interface IEmailService
{
    Task EnviarEmailConfirmacaoAsync(string email, string nome, string token);
}
