using NotificacoesService.Application.DTOs;

namespace NotificacoesService.Application.Ports.Output;

public interface IEmailGateway
{
    Task EnviarAsync(EmailMessage message, CancellationToken ct);
}
