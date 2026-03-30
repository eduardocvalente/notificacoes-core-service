namespace NotificacoesService.Infrastructure.Adapters.Input.Messaging.Events;

public sealed record MatriculaRealizadaEvent(
    Guid AlunoId,
    string EmailAluno,
    string NomeAluno,
    string NomeCurso,
    DateTime DataMatricula,
    string CorrelationId
);
