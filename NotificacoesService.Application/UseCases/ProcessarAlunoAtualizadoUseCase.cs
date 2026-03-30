using Microsoft.Extensions.Logging;
using NotificacoesService.Application.Common;
using NotificacoesService.Application.DTOs;
using NotificacoesService.Application.Ports.Input;
using NotificacoesService.Application.Ports.Output;
using NotificacoesService.Domain.Entities;
using NotificacoesService.Domain.Enums;
using NotificacoesService.Domain.Ports.Output;

namespace NotificacoesService.Application.UseCases;

public sealed class ProcessarAlunoAtualizadoUseCase : UseCaseBase, IProcessarAlunoAtualizadoUseCase
{
    private readonly INotificacaoRepository _repository;
    private readonly IEmailGateway _emailGateway;
    private readonly ITemplateRenderer _templateRenderer;

    public ProcessarAlunoAtualizadoUseCase(
        INotificacaoRepository repository,
        IEmailGateway emailGateway,
        ITemplateRenderer templateRenderer,
        ILogger<ProcessarAlunoAtualizadoUseCase> logger)
        : base(logger)
    {
        _repository = repository;
        _emailGateway = emailGateway;
        _templateRenderer = templateRenderer;
    }

    public async Task<Result> ExecutarAsync(AlunoAtualizadoInput input, CancellationToken ct)
    {
        return await ExecutarComTratamentoAsync(async () =>
        {
            // Passo 1 — Validar entrada
            var validacao = ValidarInput(input);
            if (validacao.IsFailure) return validacao;

            Logger.LogInformation(
                "Iniciando processamento de AlunoAtualizado. DestinatarioId: {DestinatarioId} | Tipo: {Tipo} | CorrelationId: {CorrelationId}",
                input.AlunoId, TipoNotificacao.AtualizacaoCadastral, input.CorrelationId);

            // Passo 2 — Renderizar template
            var corpo = await _templateRenderer.RenderizarAsync(
                TipoNotificacao.AtualizacaoCadastral, input, ct);

            // Passo 3 — Criar entidade de domínio
            var notificacao = Notificacao.Criar(
                destinatarioId: input.AlunoId,
                email: input.EmailAluno,
                tipo: TipoNotificacao.AtualizacaoCadastral,
                assunto: "Atualização Cadastral Realizada",
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
                notificacao.Id, input.AlunoId, TipoNotificacao.AtualizacaoCadastral, input.CorrelationId);

            return Result.Success();

        }, input.CorrelationId, ct);
    }

    private static Result ValidarInput(AlunoAtualizadoInput input)
    {
        if (input.AlunoId == Guid.Empty)
            return Result.Failure(Error.Validation(
                "aluno.aluno_id_invalido", "AlunoId não pode ser vazio."));

        if (string.IsNullOrWhiteSpace(input.EmailAluno))
            return Result.Failure(Error.Validation(
                "aluno.email_invalido", "E-mail do aluno é obrigatório."));

        if (string.IsNullOrWhiteSpace(input.CampoAtualizado))
            return Result.Failure(Error.Validation(
                "aluno.campo_invalido", "O campo atualizado deve ser informado."));

        return Result.Success();
    }
}
