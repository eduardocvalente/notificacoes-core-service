using Microsoft.Extensions.Logging;
using NotificacoesService.Application.Common;
using NotificacoesService.Application.DTOs;
using NotificacoesService.Application.Ports.Input;
using NotificacoesService.Application.Ports.Output;
using NotificacoesService.Domain.Entities;
using NotificacoesService.Domain.Enums;
using NotificacoesService.Domain.Ports.Output;

namespace NotificacoesService.Application.UseCases;

public sealed class ProcessarNotaLancadaUseCase : UseCaseBase, IProcessarNotaLancadaUseCase
{
    private readonly INotificacaoRepository _repository;
    private readonly IEmailGateway _emailGateway;
    private readonly ITemplateRenderer _templateRenderer;

    public ProcessarNotaLancadaUseCase(
        INotificacaoRepository repository,
        IEmailGateway emailGateway,
        ITemplateRenderer templateRenderer,
        ILogger<ProcessarNotaLancadaUseCase> logger)
        : base(logger)
    {
        _repository = repository;
        _emailGateway = emailGateway;
        _templateRenderer = templateRenderer;
    }

    public async Task<Result> ExecutarAsync(NotaLancadaInput input, CancellationToken ct)
    {
        return await ExecutarComTratamentoAsync(async () =>
        {
            // Passo 1 — Validar entrada
            var validacao = ValidarInput(input);
            if (validacao.IsFailure) return validacao;

            Logger.LogInformation(
                "Iniciando processamento de NotaLancada. DestinatarioId: {DestinatarioId} | Tipo: {Tipo} | CorrelationId: {CorrelationId}",
                input.AlunoId, TipoNotificacao.NotaDisponivel, input.CorrelationId);

            // Passo 2 — Renderizar template
            var corpo = await _templateRenderer.RenderizarAsync(
                TipoNotificacao.NotaDisponivel, input, ct);

            // Passo 3 — Criar entidade de domínio
            var notificacao = Notificacao.Criar(
                destinatarioId: input.AlunoId,
                email: input.EmailAluno,
                tipo: TipoNotificacao.NotaDisponivel,
                assunto: $"Nota disponível — {input.NomeDisciplina}",
                corpo: corpo);

            // Passo 4 — Persistir antes de enviar
            await _repository.AdicionarAsync(notificacao, ct);

            // Passo 5 — Enviar e-mail
            try
            {
                await _emailGateway.EnviarAsync(
                    new EmailMessage(input.EmailAluno, input.NomeAluno, notificacao.Assunto, corpo), ct);

                notificacao.MarcarComoEnviada();
            }
            catch (Exception ex)
            {
                notificacao.MarcarComoFalha(ex.Message);
                await _repository.AtualizarAsync(notificacao, ct);
                return Result.Failure(Error.FalhaNoEnvioEmail);
            }

            // Passo 6 — Persistir estado final
            await _repository.AtualizarAsync(notificacao, ct);

            Logger.LogInformation(
                "Notificação enviada com sucesso. NotificacaoId: {NotificacaoId} | DestinatarioId: {DestinatarioId} | Tipo: {Tipo} | CorrelationId: {CorrelationId}",
                notificacao.Id, input.AlunoId, TipoNotificacao.NotaDisponivel, input.CorrelationId);

            return Result.Success();

        }, input.CorrelationId, ct);
    }

    private static Result ValidarInput(NotaLancadaInput input)
    {
        if (input.AlunoId == Guid.Empty)
            return Result.Failure(Error.Validation(
                "nota.aluno_id_invalido", "AlunoId não pode ser vazio."));

        if (string.IsNullOrWhiteSpace(input.EmailAluno))
            return Result.Failure(Error.Validation(
                "nota.email_invalido", "E-mail do aluno é obrigatório."));

        if (string.IsNullOrWhiteSpace(input.NomeDisciplina))
            return Result.Failure(Error.Validation(
                "nota.disciplina_invalida", "Nome da disciplina é obrigatório."));

        if (input.Nota < 0 || input.Nota > 10)
            return Result.Failure(Error.Validation(
                "nota.valor_invalido", "Nota deve estar entre 0,0 e 10,0."));

        return Result.Success();
    }
}
