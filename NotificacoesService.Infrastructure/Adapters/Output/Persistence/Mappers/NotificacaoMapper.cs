using NotificacoesService.Domain.Entities;
using NotificacoesService.Domain.Enums;

namespace NotificacoesService.Infrastructure.Adapters.Output.Persistence.Mappers;

public static class NotificacaoMapper
{
    public static NotificacaoEntity ToEntity(Notificacao domain) =>
        new()
        {
            Id = domain.Id,
            DestinatarioId = domain.DestinatarioId,
            Email = domain.Email,
            Tipo = domain.Tipo.ToString(),
            Assunto = domain.Assunto,
            Corpo = domain.Corpo,
            Status = domain.Status.ToString(),
            TentativasEnvio = domain.TentativasEnvio,
            CriadaEm = domain.CriadaEm,
            EnviadaEm = domain.EnviadaEm,
            MotivoFalha = domain.MotivoFalha
        };

    public static Notificacao ToDomain(NotificacaoEntity entity) =>
        Notificacao.Reconstituir(
            id: entity.Id,
            destinatarioId: entity.DestinatarioId,
            email: entity.Email,
            tipo: Enum.Parse<TipoNotificacao>(entity.Tipo),
            assunto: entity.Assunto,
            corpo: entity.Corpo,
            status: Enum.Parse<StatusNotificacao>(entity.Status),
            tentativasEnvio: entity.TentativasEnvio,
            criadaEm: entity.CriadaEm,
            enviadaEm: entity.EnviadaEm,
            motivoFalha: entity.MotivoFalha
        );
}
