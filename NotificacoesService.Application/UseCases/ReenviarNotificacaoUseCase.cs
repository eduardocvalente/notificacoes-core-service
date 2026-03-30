using Microsoft.Extensions.Logging;
using NotificacoesService.Application.Common;
using NotificacoesService.Application.DTOs;
using NotificacoesService.Application.Ports.Input;
using NotificacoesService.Application.Ports.Output;
using NotificacoesService.Domain.Enums;
using NotificacoesService.Domain.Ports.Output;

namespace NotificacoesService.Application.UseCases;

public sealed class ReenviarNotificacaoUseCase : UseCaseBase, IReenviarNotificacaoUseCase
{
    private readonly INotificacaoRepository _repository;
    private readonly IEmailGateway _emailGateway;
    private readonly ITemplateRenderer _templateRenderer;

    public ReenviarNotificacaoUseCase(
        INotificacaoRepository repository,
        IEmailGateway emailGateway,
        ITemplateRenderer templateRenderer,
        ILogger<ReenviarNotificacaoUseCase> logger)
        : base(logger)
    {
        _repository = repository;
        _emailGateway = emailGateway;
        _templateRenderer = templateRenderer;
    }

    public async Task<Result> ExecutarAsync(ReenviarNotificacaoInput input, CancellationToken ct)
    {
        return await ExecutarComTratamentoAsync(async () =>
        {
            Logger.LogInformation(
                "Iniciando reenvio. NotificacaoId: {NotificacaoId} | CorrelationId: {CorrelationId}",
                input.NotificacaoId, input.CorrelationId);

            // Passo 1 — Buscar notificação
            var notificacao = await _repository.ObterPorIdAsync(input.NotificacaoId, ct);
            if (notificacao is null)
                return Result.Failure(Error.NotificacaoNaoEncontrada);

            // Passo 2 — Validar regras de domínio
            if (notificacao.Status == StatusNotificacao.Enviada)
                return Result.Failure(Error.NotificacaoJaEnviada);

            if (!notificacao.PodeRetentar())
                return Result.Failure(Error.LimiteRetentativasAtingido);

            Logger.LogInformation(
                "Reenvio autorizado. NotificacaoId: {NotificacaoId} | DestinatarioId: {DestinatarioId} | Tipo: {Tipo} | CorrelationId: {CorrelationId}",
                input.NotificacaoId, notificacao.DestinatarioId, notificacao.Tipo, input.CorrelationId);

            // Passo 3 — Renderizar template com os dados da notificação persistida
            var corpo = await _templateRenderer.RenderizarAsync(notificacao.Tipo, notificacao, ct);

            var emailMessage = new EmailMessage(
                Para: notificacao.Email,
                NomeDestinatario: notificacao.Email,
                Assunto: notificacao.Assunto,
                Corpo: corpo);

            // Passo 4 — Enviar e-mail
            try
            {
                await _emailGateway.EnviarAsync(emailMessage, ct);
                notificacao.MarcarComoEnviada();
            }
            catch (Exception ex)
            {
                notificacao.MarcarComoFalha(ex.Message);
                await _repository.AtualizarAsync(notificacao, ct);
                return Result.Failure(Error.FalhaNoEnvioEmail);
            }

            // Passo 5 — Persistir estado final
            await _repository.AtualizarAsync(notificacao, ct);

            Logger.LogInformation(
                "Reenvio concluído. NotificacaoId: {NotificacaoId} | DestinatarioId: {DestinatarioId} | Tipo: {Tipo} | CorrelationId: {CorrelationId}",
                input.NotificacaoId, notificacao.DestinatarioId, notificacao.Tipo, input.CorrelationId);

            return Result.Success();

        }, input.CorrelationId, ct);
    }
}
