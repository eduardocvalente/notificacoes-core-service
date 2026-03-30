using NotificacoesService.Application.Common;
using NotificacoesService.Application.DTOs;

namespace NotificacoesService.Application.Ports.Input;

public interface IConsultarNotificacoesUseCase
{
    Task<Result<IReadOnlyList<NotificacaoResponse>>> ExecutarAsync(
        ConsultarNotificacoesInput input,
        CancellationToken ct);
}
