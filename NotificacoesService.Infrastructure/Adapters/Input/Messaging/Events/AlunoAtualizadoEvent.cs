namespace NotificacoesService.Infrastructure.Adapters.Input.Messaging.Events;

public sealed record AlunoAtualizadoEvent(
    Guid AlunoId,
    string EmailAluno,
    string NomeAluno,
    string CampoAtualizado,
    DateTime DataAtualizacao,
    string CorrelationId
);
