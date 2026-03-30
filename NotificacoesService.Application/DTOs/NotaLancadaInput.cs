namespace NotificacoesService.Application.DTOs;

public sealed record NotaLancadaInput(
    Guid AlunoId,
    string EmailAluno,
    string NomeAluno,
    string NomeDisciplina,
    decimal Nota,
    DateTime DataLancamento,
    string CorrelationId = "");
