using NotificacoesService.Domain.Enums;
using NotificacoesService.Domain.Exceptions;

namespace NotificacoesService.Domain.Entities;

public sealed class Notificacao
{
    public Guid Id { get; private set; }
    public Guid DestinatarioId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public TipoNotificacao Tipo { get; private set; }
    public string Assunto { get; private set; } = string.Empty;
    public string Corpo { get; private set; } = string.Empty;
    public StatusNotificacao Status { get; private set; }
    public int TentativasEnvio { get; private set; }
    public DateTime CriadaEm { get; private set; }
    public DateTime? EnviadaEm { get; private set; }
    public string? MotivoFalha { get; private set; }

    private Notificacao() { }

    public static Notificacao Criar(
        Guid destinatarioId,
        string email,
        TipoNotificacao tipo,
        string assunto,
        string corpo)
    {
        return new Notificacao
        {
            Id = Guid.NewGuid(),
            DestinatarioId = destinatarioId,
            Email = email,
            Tipo = tipo,
            Assunto = assunto,
            Corpo = corpo,
            Status = StatusNotificacao.Pendente,
            TentativasEnvio = 0,
            CriadaEm = DateTime.UtcNow
        };
    }

    public static Notificacao Reconstituir(
        Guid id,
        Guid destinatarioId,
        string email,
        TipoNotificacao tipo,
        string assunto,
        string corpo,
        StatusNotificacao status,
        int tentativasEnvio,
        DateTime criadaEm,
        DateTime? enviadaEm,
        string? motivoFalha)
    {
        return new Notificacao
        {
            Id = id,
            DestinatarioId = destinatarioId,
            Email = email,
            Tipo = tipo,
            Assunto = assunto,
            Corpo = corpo,
            Status = status,
            TentativasEnvio = tentativasEnvio,
            CriadaEm = criadaEm,
            EnviadaEm = enviadaEm,
            MotivoFalha = motivoFalha
        };
    }

    public void MarcarComoEnviada()
    {
        if (Status == StatusNotificacao.Enviada)
            throw new NotificacaoJaEnviadaException(Id);

        Status = StatusNotificacao.Enviada;
        EnviadaEm = DateTime.UtcNow;
        MotivoFalha = null;
    }

    public void MarcarComoFalha(string motivo)
    {
        TentativasEnvio++;
        Status = StatusNotificacao.Falha;
        MotivoFalha = motivo;
    }

    public bool PodeRetentar() => TentativasEnvio < 3;
}
