using Microsoft.Extensions.Logging;
using NotificacoesService.Application.Common;
using NotificacoesService.Application.DTOs;
using NotificacoesService.Application.Ports.Input;
using NotificacoesService.Domain.Entities;
using NotificacoesService.Domain.Ports.Output;

namespace NotificacoesService.Application.UseCases;

public sealed class ConsultarNotificacoesUseCase : UseCaseBase, IConsultarNotificacoesUseCase
{
    private readonly INotificacaoRepository _repository;

    public ConsultarNotificacoesUseCase(
        INotificacaoRepository repository,
        ILogger<ConsultarNotificacoesUseCase> logger)
        : base(logger)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<NotificacaoResponse>>> ExecutarAsync(
        ConsultarNotificacoesInput input,
        CancellationToken ct)
    {
        return await ExecutarComTratamentoAsync<IReadOnlyList<NotificacaoResponse>>(async () =>
        {
            Logger.LogInformation(
                "Consultando notificações. DestinatarioId: {DestinatarioId} | CorrelationId: {CorrelationId}",
                input.DestinatarioId, input.CorrelationId);

            var notificacoes = await _repository.ListarPorDestinatarioAsync(input.DestinatarioId, ct);
            var response = notificacoes.Select(ToResponse).ToList().AsReadOnly();

            Logger.LogInformation(
                "Consulta concluída. DestinatarioId: {DestinatarioId} | Total: {Total} | CorrelationId: {CorrelationId}",
                input.DestinatarioId, response.Count, input.CorrelationId);

            return Result<IReadOnlyList<NotificacaoResponse>>.Success(response);

        }, input.CorrelationId, ct);
    }

    private static NotificacaoResponse ToResponse(Notificacao n) =>
        new(
            Id: n.Id,
            DestinatarioId: n.DestinatarioId,
            Email: n.Email,
            Tipo: n.Tipo.ToString(),
            Assunto: n.Assunto,
            Status: n.Status.ToString(),
            TentativasEnvio: n.TentativasEnvio,
            CriadaEm: n.CriadaEm,
            EnviadaEm: n.EnviadaEm,
            MotivoFalha: n.MotivoFalha);
}
