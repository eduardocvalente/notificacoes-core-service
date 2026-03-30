using NotificacoesService.Application.Common;
using NotificacoesService.Application.DTOs;

namespace NotificacoesService.Application.Ports.Input;

public interface IProcessarMatriculaRealizadaUseCase
{
    Task<Result> ExecutarAsync(MatriculaRealizadaInput input, CancellationToken ct);
}
