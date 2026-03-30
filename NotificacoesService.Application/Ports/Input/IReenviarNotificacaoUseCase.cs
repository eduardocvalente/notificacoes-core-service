using NotificacoesService.Application.Common;
using NotificacoesService.Application.DTOs;

namespace NotificacoesService.Application.Ports.Input;

public interface IReenviarNotificacaoUseCase
{
    Task<Result> ExecutarAsync(ReenviarNotificacaoInput input, CancellationToken ct);
}
