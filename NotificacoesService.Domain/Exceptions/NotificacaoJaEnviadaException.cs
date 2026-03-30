namespace NotificacoesService.Domain.Exceptions;

public sealed class NotificacaoJaEnviadaException : DomainException
{
    public Guid NotificacaoId { get; }

    public NotificacaoJaEnviadaException(Guid notificacaoId)
        : base($"A notificação '{notificacaoId}' já foi enviada e não pode ser reenviada diretamente.")
    {
        NotificacaoId = notificacaoId;
    }
}
