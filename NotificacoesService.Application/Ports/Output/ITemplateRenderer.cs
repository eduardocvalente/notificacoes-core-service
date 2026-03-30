using NotificacoesService.Domain.Enums;

namespace NotificacoesService.Application.Ports.Output;

public interface ITemplateRenderer
{
    Task<string> RenderizarAsync(TipoNotificacao tipo, object dados, CancellationToken ct);
}
