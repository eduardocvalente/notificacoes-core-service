namespace NotificacoesService.Application.DTOs;

public sealed record ConsultarNotificacoesInput(
    Guid DestinatarioId,
    string CorrelationId);
