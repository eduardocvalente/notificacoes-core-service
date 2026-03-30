namespace NotificacoesService.Infrastructure.Adapters.Input.Messaging.Events;

public sealed record NotaLancadaEvent(
    Guid AlunoId,
    string EmailAluno,
    string NomeAluno,
    string NomeDisciplina,
    decimal Nota,
    DateTime DataLancamento,
    string CorrelationId
);
