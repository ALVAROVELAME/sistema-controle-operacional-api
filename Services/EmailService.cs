using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace SistemaControleOperacionalApi.Services;

/// <summary>
/// Espelha EmailService (Spring Mail) usando MailKit.
/// Envia e-mail de confirmacao com link para o FRONTEND.
/// </summary>
public sealed class EmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _from;

    public EmailService()
    {
        _host = Env("MAIL_HOST");
        _port = int.Parse(Env("MAIL_PORT"));
        _username = Env("MAIL_USERNAME");
        _password = Env("MAIL_PASSWORD");
        _from = Env("MAIL_FROM");
    }

    public async Task EnviarEmailConfirmacaoAsync(
        string email,
        string nome,
        string token)
    {
        var frontendUrl = Environment.GetEnvironmentVariable("APP_FRONTEND_URL")
            ?? "http://localhost:5173";

        // O link aponta para o FRONTEND (mesma decisao do Java).
        var link = $"{frontendUrl}/confirmar?token={token}";

        var mensagem = new MimeMessage();
        mensagem.From.Add(MailboxAddress.Parse(_from));
        mensagem.To.Add(MailboxAddress.Parse(email));
        mensagem.Subject = "Confirmacao de cadastro - Sistema Controle Operacional";

        mensagem.Body = new TextPart(TextFormat.Plain)
        {
            Text = $"""
                Ola {nome},

                Seja bem-vindo ao Sistema de Controle Operacional!

                Para ativar sua conta, clique no link abaixo:

                {link}

                Este link e valido por 24 horas.

                Se voce nao criou esta conta, ignore este email.

                Equipe Sistema Controle Operacional
                """
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_username, _password);
        await client.SendAsync(mensagem);
        await client.DisconnectAsync(true);
    }

    private static string Env(string key) =>
        Environment.GetEnvironmentVariable(key)
        ?? throw new InvalidOperationException(
            $"{key} nao configurado no ambiente.");
}