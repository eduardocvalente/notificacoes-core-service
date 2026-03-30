namespace NotificacoesService.Application.DTOs;

public sealed record ReenviarNotificacaoInput(
    Guid NotificacaoId,
    string CorrelationId);
