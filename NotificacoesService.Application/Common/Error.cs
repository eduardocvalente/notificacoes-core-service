namespace NotificacoesService.Application.Common;

public sealed record Error(
    string Code,
    string Description,
    ErrorType Type)
{
    // ── Factory methods ──────────────────────────────────────────────────────

    public static Error Validation(string code, string description)
        => new(code, description, ErrorType.Validation);

    public static Error NotFound(string code, string description)
        => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description)
        => new(code, description, ErrorType.Conflict);

    public static Error Unexpected(string code, string description)
        => new(code, description, ErrorType.Unexpected);

    // ── Erros de domínio pré-definidos ───────────────────────────────────────

    public static readonly Error NotificacaoNaoEncontrada =
        NotFound("notificacao.nao_encontrada", "Notificação não encontrada.");

    public static readonly Error NotificacaoJaEnviada =
        Conflict("notificacao.ja_enviada", "A notificação já foi enviada e não pode ser reenviada.");

    public static readonly Error LimiteRetentativasAtingido =
        Conflict("notificacao.limite_retentativas", "O limite de tentativas de envio foi atingido.");

    public static readonly Error FalhaNoEnvioEmail =
        Unexpected("email.falha_envio", "Falha ao enviar e-mail via gateway SMTP.");
}
