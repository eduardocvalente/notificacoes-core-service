using NotificacoesService.Domain.Entities;

namespace NotificacoesService.Domain.Ports.Output;

public interface INotificacaoRepository
{
    Task<Notificacao?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task AdicionarAsync(Notificacao notificacao, CancellationToken ct);
    Task AtualizarAsync(Notificacao notificacao, CancellationToken ct);
    Task<IReadOnlyList<Notificacao>> ListarPorDestinatarioAsync(Guid destinatarioId, CancellationToken ct);
    Task<IReadOnlyList<Notificacao>> ListarPendentesPorTentativaAsync(int maxTentativas, CancellationToken ct);
}
