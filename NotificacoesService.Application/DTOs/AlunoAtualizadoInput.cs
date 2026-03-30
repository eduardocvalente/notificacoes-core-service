namespace NotificacoesService.Application.DTOs;

public sealed record AlunoAtualizadoInput(
    Guid AlunoId,
    string EmailAluno,
    string NomeAluno,
    string CampoAtualizado,
    DateTime DataAtualizacao,
    string CorrelationId = "");
