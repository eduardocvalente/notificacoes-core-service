using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificacoesService.Application.Common;

namespace NotificacoesService.Infrastructure.Adapters.Input.Messaging;

/// <summary>
/// Classe base para consumers de mensageria. Define o loop de consumo,
/// tratamento de falha e log. Subclasses implementam apenas
/// <see cref="ReceberEventosAsync"/> e <see cref="ProcessarEventoAsync"/>.
/// </summary>
public abstract class ConsumerBase<TEvent> : BackgroundService
{
    protected readonly ILogger Logger;
    protected readonly IServiceScopeFactory ScopeFactory;

    protected ConsumerBase(ILogger logger, IServiceScopeFactory scopeFactory)
    {
        Logger = logger;
        ScopeFactory = scopeFactory;
    }

    // ── Template Method ───────────────────────────────────────────────────────

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Consumer {Consumer} iniciado.", GetType().Name);

        await foreach (var evento in ReceberEventosAsync(stoppingToken))
        {
            var correlationId = Guid.NewGuid().ToString();

            using var scope = ScopeFactory.CreateScope();

            try
            {
                Logger.LogInformation(
                    "Processando evento {EventType}. CorrelationId: {CorrelationId}",
                    typeof(TEvent).Name, correlationId);

                var resultado = await ProcessarEventoAsync(
                    evento, scope.ServiceProvider, correlationId, stoppingToken);

                if (resultado.IsFailure)
                {
                    Logger.LogWarning(
                        "Evento processado com falha. Código: {Codigo} | Descrição: {Descricao} | CorrelationId: {CorrelationId}",
                        resultado.Error.Code, resultado.Error.Description, correlationId);
                }
                else
                {
                    Logger.LogInformation(
                        "Evento processado com sucesso. CorrelationId: {CorrelationId}", correlationId);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Exceção não tratada ao processar evento {EventType}. CorrelationId: {CorrelationId}",
                    typeof(TEvent).Name, correlationId);
            }
        }

        Logger.LogInformation("Consumer {Consumer} encerrado.", GetType().Name);
    }

    // ── Métodos abstratos ─────────────────────────────────────────────────────

    /// <summary>
    /// Conecta ao broker e expõe os eventos como stream assíncrono.
    /// ACK/NACK ocorre dentro deste método, antes de yield.
    /// </summary>
    protected abstract IAsyncEnumerable<TEvent> ReceberEventosAsync(CancellationToken ct);

    /// <summary>
    /// Mapeia o evento recebido para o Input DTO e invoca o Use Case correspondente.
    /// Nunca deve conter lógica de negócio.
    /// </summary>
    protected abstract Task<Result> ProcessarEventoAsync(
        TEvent evento,
        IServiceProvider services,
        string correlationId,
        CancellationToken ct);
}
