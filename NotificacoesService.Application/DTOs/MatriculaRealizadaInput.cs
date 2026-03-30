namespace NotificacoesService.Application.DTOs;

public sealed record MatriculaRealizadaInput(
    Guid AlunoId,
    string EmailAluno,
    string NomeAluno,
    string NomeCurso,
    DateTime DataMatricula,
    string CorrelationId = "");
