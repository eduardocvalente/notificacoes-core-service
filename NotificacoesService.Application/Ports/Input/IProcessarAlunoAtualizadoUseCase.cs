using NotificacoesService.Application.Common;
using NotificacoesService.Application.DTOs;

namespace NotificacoesService.Application.Ports.Input;

public interface IProcessarAlunoAtualizadoUseCase
{
    Task<Result> ExecutarAsync(AlunoAtualizadoInput input, CancellationToken ct);
}
