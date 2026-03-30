using Microsoft.Extensions.Logging;
using NotificacoesService.Application.Common;
using NotificacoesService.Application.DTOs;
using NotificacoesService.Application.Ports.Input;
using NotificacoesService.Application.Ports.Output;
using NotificacoesService.Domain.Entities;
using NotificacoesService.Domain.Enums;
using NotificacoesService.Domain.Ports.Output;

namespace NotificacoesService.Application.UseCases;

public sealed class ProcessarMatriculaRealizadaUseCase : UseCaseBase, IProcessarMatriculaRealizadaUseCase
{
    private readonly INotificacaoRepository _repository;
    private readonly IEmailGateway _emailGateway;
    private readonly ITemplateRenderer _templateRenderer;

    public ProcessarMatriculaRealizadaUseCase(
        INotificacaoRepository repository,
        IEmailGateway emailGateway,
        ITemplateRenderer templateRenderer,
        ILogger<ProcessarMatriculaRealizadaUseCase> logger)
        : base(logger)
    {
        _repository = repository;
        _emailGateway = emailGateway;
        _templateRenderer = templateRenderer;
    }

    public async Task<Result> ExecutarAsync(MatriculaRealizadaInput input, CancellationToken ct)
    {
        return await ExecutarComTratamentoAsync(async () =>
        {
            // Passo 1 — Validar entrada
            var validacao = ValidarInput(input);
            if (validacao.IsFailure) return validacao;

            Logger.LogInformation(
                "Iniciando processamento de MatriculaRealizada. DestinatarioId: {DestinatarioId} | Tipo: {Tipo} | CorrelationId: {CorrelationId}",
                input.AlunoId, TipoNotificacao.MatriculaConfirmada, input.CorrelationId);

            // Passo 2 — Renderizar template
            var corpo = await _templateRenderer.RenderizarAsync(
                TipoNotificacao.MatriculaConfirmada, input, ct);

            // Passo 3 — Criar entidade de domínio
            var notificacao = Notificacao.Criar(
                destinatarioId: input.AlunoId,
                email: input.EmailAluno,
                tipo: TipoNotificacao.MatriculaConfirmada,
                assunto: "Confirmação de Matrícula",
                corpo: corpo);

            // Passo 4 — Persistir antes de enviar (garante histórico mesmo em falha de e-mail)
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
                notificacao.Id, input.AlunoId, TipoNotificacao.MatriculaConfirmada, input.CorrelationId);

            return Result.Success();

        }, input.CorrelationId, ct);
    }

    private static Result ValidarInput(MatriculaRealizadaInput input)
    {
        if (input.AlunoId == Guid.Empty)
            return Result.Failure(Error.Validation(
                "matricula.aluno_id_invalido", "AlunoId não pode ser vazio."));

        if (string.IsNullOrWhiteSpace(input.EmailAluno))
            return Result.Failure(Error.Validation(
                "matricula.email_invalido", "E-mail do aluno é obrigatório."));

        if (string.IsNullOrWhiteSpace(input.NomeCurso))
            return Result.Failure(Error.Validation(
                "matricula.curso_invalido", "Nome do curso é obrigatório."));

        return Result.Success();
    }
}
