using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using NotificacoesService.Application.DTOs;
using NotificacoesService.Application.Options;
using NotificacoesService.Application.Ports.Output;

namespace NotificacoesService.Infrastructure.Adapters.Output.Email;

public sealed class SmtpEmailGateway : IEmailGateway
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailGateway> _logger;

    public SmtpEmailGateway(
        IOptions<SmtpOptions> options,
        ILogger<SmtpEmailGateway> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnviarAsync(EmailMessage message, CancellationToken ct)
    {
        _logger.LogInformation(
            "Enviando e-mail via SMTP. Para: {Para}, Assunto: {Assunto}",
            message.Para,
            message.Assunto);

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("Sistema Escolar", _options.From));
        mimeMessage.To.Add(new MailboxAddress(message.NomeDestinatario, message.Para));
        mimeMessage.Subject = message.Assunto;
        mimeMessage.Body = new TextPart(TextFormat.Html) { Text = message.Corpo };

        using var client = new SmtpClient();

        await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.Auto, ct);
        await client.AuthenticateAsync(_options.Username, _options.Password, ct);
        await client.SendAsync(mimeMessage, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation(
            "E-mail enviado com sucesso. Para: {Para}",
            message.Para);
    }
}
