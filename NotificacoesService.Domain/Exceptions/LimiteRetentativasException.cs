namespace NotificacoesService.Domain.Exceptions;

public sealed class LimiteRetentativasException : DomainException
{
    public Guid NotificacaoId { get; }
    public int TentativasRealizadas { get; }

    public LimiteRetentativasException(Guid notificacaoId, int tentativasRealizadas)
        : base($"A notificação '{notificacaoId}' atingiu o limite de {tentativasRealizadas} tentativas de envio.")
    {
        NotificacaoId = notificacaoId;
        TentativasRealizadas = tentativasRealizadas;
    }
}
