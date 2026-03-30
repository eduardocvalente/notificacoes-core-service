using NotificacoesService.Application.Common;
using NotificacoesService.Application.DTOs;

namespace NotificacoesService.Application.Ports.Input;

public interface IProcessarNotaLancadaUseCase
{
    Task<Result> ExecutarAsync(NotaLancadaInput input, CancellationToken ct);
}
